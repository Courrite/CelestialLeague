using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection.Metadata;
using System.Threading;
using System.Threading.Tasks;
using Celeste.Mod;
using CelestialLeague.Shared.Enum;
using CelestialLeague.Shared.Packets;
using CelestialLeague.Shared.Utils;

namespace CelestialLeague.Client.Networking
{
    public class ConnectionManager : IDisposable
    {
        private readonly GameClient _gameClient;
        private readonly NetworkClient _networkClient;

        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly ConcurrentDictionary<uint?, PendingRequest> _pendingRequests;
        private NetworkStream _stream;
        private Task _receiveTask;
        private Task _heartbeatTask;

        private bool _isConnected;
        private DateTime? _connectedAt;
        private DateTime _lastActivity;
        private string _serverEndpoint;
        private bool _disposed;

        public bool IsConnected => _isConnected && !_disposed;
        public DateTime? ConnectedAt => _connectedAt;
        public DateTime LastActivity => _lastActivity;
        public string ServerEndpoint => _serverEndpoint;
        public int PendingRequestCount => _pendingRequests.Count;

        public event EventHandler<PacketReceivedEventArgs> OnPacketReceived;
        public event EventHandler<DisconnectedEventArgs> OnDisconnected;

        public ConnectionManager(GameClient gameClient, NetworkClient networkClient)
        {
            _gameClient = gameClient ?? throw new ArgumentNullException(nameof(gameClient));
            _networkClient = networkClient ?? throw new ArgumentNullException(nameof(networkClient));
            _cancellationTokenSource = new CancellationTokenSource();
            _pendingRequests = new ConcurrentDictionary<uint?, PendingRequest>();
            _lastActivity = DateTime.UtcNow;
        }

        public async Task StartAsync(NetworkStream stream, string serverEndpoint)
        {
            _stream = stream;
            _serverEndpoint = serverEndpoint;
            _isConnected = true;
            _connectedAt = DateTime.UtcNow;
            _lastActivity = _connectedAt.Value;

            _receiveTask = ReceiveLoopAsync(_cancellationTokenSource.Token);
            _heartbeatTask = HeartbeatLoopAsync(_cancellationTokenSource.Token);

            await Task.Delay(100);

            if (_receiveTask.IsFaulted)
                throw new InvalidOperationException("Failed to start receive loop", _receiveTask.Exception);

            if (_heartbeatTask.IsFaulted)
                throw new InvalidOperationException("Failed to start heartbeat loop", _heartbeatTask.Exception);
        }


        public async Task StopAsync(string reason = "Connection stopped")
        {
            if (_disposed || !_isConnected)
                return;

            _isConnected = false;

            _cancellationTokenSource.Cancel();

            try
            {
                if (_receiveTask != null)
                    await _receiveTask;

                if (_heartbeatTask != null)
                    await _heartbeatTask;
            }
            catch (OperationCanceledException)
            {
                // task was canceled, ignore
            }
            catch (Exception ex)
            {
                Logger.Warn("Celestial League", "Exception during task cleanup: " + ex.Message);
            }

            var pendingRequests = _pendingRequests.Values.ToArray();
            foreach (var request in pendingRequests)
            {
                try
                {
                    request.CompletionSource.SetResult(null);
                    request.CancellationRegistration.Dispose();
                }
                catch (InvalidOperationException)
                {
                    // reg already completed, ignroe
                }
            }

            try
            {
                _stream?.Close();
                _stream?.Dispose();
                _stream = null;
            }
            catch (Exception ex)
            {
                Logger.Warn("Celestial League", "Exception during task cleanup: " + ex.Message);
            }

            _connectedAt = null;
            _serverEndpoint = null;

            OnDisconnected?.Invoke(this, new DisconnectedEventArgs(reason));
        }

        public async Task<TResponse> SendRequestAsync<TRequest, TResponse>(TRequest request, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
        where TRequest : BasePacket where TResponse : BasePacket
        {
            if (!IsConnected)
                throw new InvalidOperationException("Not connected to server");

            if (request.CorrelationId == 0)
                throw new InvalidOperationException("Request packet must have a correlation ID");

            var correlationId = request.CorrelationId;

            var timeoutMs = (int)(timeout?.TotalMilliseconds ?? Shared.Constants.Network.SocketTimeoutMs);
            using var timeoutCts = new CancellationTokenSource(timeoutMs);
            using var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken,
                timeoutCts.Token,
                _cancellationTokenSource.Token);

            var completionSource = new TaskCompletionSource<BasePacket>();
            var pendingRequest = new PendingRequest(completionSource, cancellationToken)
            {
                CorrelationId = correlationId,
                RequestTime = DateTime.UtcNow,
                ExpectedResponseType = typeof(TResponse)
            };

            pendingRequest.CancellationRegistration = combinedCts.Token.Register(() =>
            {
                _pendingRequests.TryRemove(correlationId, out _);

                if (timeoutCts.Token.IsCancellationRequested)
                {
                    completionSource.TrySetResult(null);
                }
                else
                {
                    completionSource.TrySetCanceled(combinedCts.Token);
                }
            });

            if (!_pendingRequests.TryAdd(correlationId, pendingRequest))
            {
                pendingRequest.CancellationRegistration.Dispose();
                throw new InvalidOperationException($"Duplicate correlation ID: {correlationId}");
            }

            try
            {
                var sent = await _networkClient.SendPacketAsync<TRequest>(request, _stream);
                if (!sent)
                {
                    _pendingRequests.TryRemove(correlationId, out _);
                    return null;
                }

                var response = await completionSource.Task;

                return response as TResponse;
            }
            catch (OperationCanceledException) when (timeoutCts.Token.IsCancellationRequested)
            {
                // timeout
                return null;
            }
            catch (OperationCanceledException)
            {
                // user or connection cancellation, rethrow
                throw;
            }
            finally
            {
                // cleanup
                _pendingRequests.TryRemove(correlationId, out _);
                pendingRequest.CancellationRegistration.Dispose();
            }
        }

        public async Task<bool> SendPacketAsync<T>(T packet) where T : BasePacket
        {
            if (!IsConnected || _stream == null)
                return false;

            try
            {
                await _networkClient.SendPacketAsync<T>(packet, _stream);
                return true;
            }
            catch (ObjectDisposedException)
            {
                // steam was disposed, connection is dead
                _isConnected = false;
                return false;
            }
            catch (SocketException)
            {
                // network error, connection might be dead
                _isConnected = false;
                return false;
            }
            catch (InvalidOperationException)
            {
                // stream not writable or other state issues
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public Task HandleReceivedPacketAsync(BasePacket packet, int actualPacketSize)
        {
            if (packet == null)
                return Task.CompletedTask;

            _lastActivity = DateTime.UtcNow;

            if (TryCompleteRequest(packet))
                return Task.CompletedTask;

            OnPacketReceived?.Invoke(this, new PacketReceivedEventArgs(packet, actualPacketSize));
            return Task.CompletedTask;
        }

        private bool TryCompleteRequest(BasePacket packet)
        {
            if (packet?.CorrelationId == null || packet.CorrelationId == 0)
                return false;

            if (!_pendingRequests.TryRemove(packet.CorrelationId, out var request))
                return false;

            try
            {
                request.CompletionSource.TrySetResult(packet);
                return true;
            }
            finally
            {
                request.CancellationRegistration.Dispose();
            }
        }

        private async Task ReceiveLoopAsync(CancellationToken cancellationToken)
        {
            var lengthBuffer = new byte[4];

            try
            {
                while (!cancellationToken.IsCancellationRequested && _stream != null)
                {
                    try
                    {
                        var headerBytesRead = await ReadExactAsync(_stream, lengthBuffer, 4, cancellationToken);
                        if (headerBytesRead != 4)
                        {
                            // connection closed
                            break;
                        }

                        var packetLength = BitConverter.ToInt32(lengthBuffer, 0);
                        if (packetLength <= 0 || packetLength > Shared.Constants.Network.MaxPacketSize)
                        {
                            // invalid packet length, connection closed
                            break;
                        }

                        var packetBuffer = new byte[packetLength];
                        var packetBytesRead = await ReadExactAsync(_stream, packetBuffer, packetLength, cancellationToken);
                        if (packetBytesRead != packetLength)
                        {
                            // connection closed
                            break;
                        }

                        await ProcessReceivedPacketAsync(packetBuffer, packetLength);
                    }
                    catch (OperationCanceledException)
                    {
                        // expected when cancellation is requested
                        break;
                    }
                    catch (ObjectDisposedException)
                    {
                        // stream was disposed
                        break;
                    }
                    catch (SocketException)
                    {
                        break;
                    }
                    catch (IOException)
                    {
                        // network error, usually wraps SocketException
                        break;
                    }
                    catch (InvalidOperationException)
                    {
                        // dont break loop for malformed packets
                        continue;
                    }
                    catch (Exception)
                    {
                        await Task.Delay(100, cancellationToken);
                    }
                }
            }
            catch (Exception ex) when (!(ex is OperationCanceledException))
            {
                _ = Task.Run(() => StopAsync($"Receive loop error: {ex.Message}"));
            }
        }

        private async Task<int> ReadExactAsync(NetworkStream stream, byte[] buffer, int count, CancellationToken cancellationToken)
        {
            if (stream == null || buffer == null)
                return 0;

            if (count <= 0 || count > buffer.Length)
                return 0;

            int totalBytesRead = 0;
            int bytesRemaining = count;

            while (bytesRemaining > 0 && !cancellationToken.IsCancellationRequested)
            {
                try
                {
                    int bytesRead = await stream.ReadAsync(buffer, totalBytesRead, bytesRemaining, cancellationToken);
                    if (bytesRead == 0)
                    {
                        // connection closed
                        break;
                    }

                    totalBytesRead += bytesRead;
                    bytesRemaining -= bytesRead;
                }
                catch (OperationCanceledException)
                {
                    // expected when cancellation is requested
                    break;
                }
                catch (ObjectDisposedException)
                {
                    // stream was disposed
                    break;
                }
                catch (IOException)
                {
                    // network error
                    break;
                }
            }

            return totalBytesRead;
        }

        private async Task ProcessReceivedPacketAsync(byte[] data, int length)
        {
            try
            {
                var packet = Serialization.DeserializePacket(data, length);
                _lastActivity = DateTime.UtcNow;
                await HandleReceivedPacketAsync(packet, length);
            }
            catch (InvalidOperationException ex)
            {
                Logger.Warn("Celestial League", $"Failed to deserialize received packet ({length} bytes): {ex.Message}");
                _lastActivity = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                Logger.Error("Celestial League", $"Unexpected error processing packet: {ex.Message}");
                _lastActivity = DateTime.UtcNow;
            }
        }

        public ConnectionQuality GetConnectionQuality() => _networkClient.CalculateConnectionQuality();
        public NetworkStatistics GetNetworkStatistics() => _networkClient.GetStatistics();

        private async Task HeartbeatLoopAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested && IsConnected)
            {
                try
                {
                    await Task.Delay(Shared.Constants.Network.HeartbeatIntervalMs, cancellationToken);

                    if (!IsConnected)
                        break;

                    var success = await SendPacketAsync(new HeartbeatPacket());
                    if (!success)
                    {
                        break;
                    }

                    CleanupExpiredRequests();

                    var timeSinceLastActivity = DateTime.UtcNow - _lastActivity;
                    if (timeSinceLastActivity > TimeSpan.FromMilliseconds(Shared.Constants.Network.ConnectionTimeoutMs))
                    {
                        // connection appears dead - no activity for too long
                        break;
                    }
                }
                catch (OperationCanceledException)
                {
                    // expected when cancellation is requested
                    break;
                }
                catch (ObjectDisposedException)
                {
                    // connection was disposed
                    break;
                }
                catch (Exception)
                {
                    // other errors - brief pause then continue
                    try
                    {
                        await Task.Delay(1000, cancellationToken);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                }
            }
        }

        private void CleanupExpiredRequests()
        {
            var cutoffTime = DateTime.UtcNow.AddSeconds(-Shared.Constants.Network.SocketTimeoutMs);

            var expiredKeys = new List<uint?>();
            foreach (var kvp in _pendingRequests)
            {
                if (kvp.Value.RequestTime <= cutoffTime)
                {
                    expiredKeys.Add(kvp.Key);
                }
            }

            foreach (var key in expiredKeys)
            {
                if (_pendingRequests.TryRemove(key, out var expiredRequest))
                {
                    try
                    {
                        expiredRequest.CompletionSource.TrySetResult(null);
                        expiredRequest.CancellationRegistration.Dispose();
                    }
                    catch (ObjectDisposedException)
                    {
                        // alraedy disposed
                    }
                }
            }
        }
        
        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            try
            {
                var stopTask = StopAsync("Connection disposed");

                if (!stopTask.Wait(TimeSpan.FromSeconds(2)))
                {
                    _cancellationTokenSource?.Cancel();
                }

                _cancellationTokenSource?.Dispose();
            }
            catch (Exception)
            {
                // disposal should never throw
            }
        }
    }

    public class PendingRequest
    {
        public uint? CorrelationId { get; set; }
        public TaskCompletionSource<BasePacket> CompletionSource { get; set; } = null!;
        public DateTime RequestTime { get; set; }
        public Type ExpectedResponseType { get; set; } = null!;
        public CancellationTokenRegistration CancellationRegistration { get; set; }

        public PendingRequest(TaskCompletionSource<BasePacket> completionSource, CancellationToken cancellationToken)
        {
            CompletionSource = completionSource;
            RequestTime = DateTime.UtcNow;
            CancellationRegistration = cancellationToken.Register(() => CompletionSource.TrySetCanceled());
        }
    }

    public class PacketReceivedEventArgs : EventArgs
    {
        public BasePacket Packet { get; }
        public PacketType PacketType { get; }
        public DateTime ReceivedAt { get; }
        public int PacketSize { get; }
        public uint? CorrelationId { get; }

        public PacketReceivedEventArgs(BasePacket packet, int packetSize)
        {
            Packet = packet ?? throw new ArgumentNullException(nameof(packet));
            PacketType = packet.Type;
            ReceivedAt = DateTime.UtcNow;
            PacketSize = packetSize;
            CorrelationId = packet.CorrelationId;
        }

        public bool IsPacketType<T>() where T : BasePacket => Packet is T;
        public bool IsPacketType(PacketType packetType) => Packet.Type == packetType;
    }

    public class DisconnectedEventArgs : EventArgs
    {
        public string Message { get; }
        public Exception Exception { get; }
        public DateTime DisconnectedAt { get; }
        public ResponseCode? ResponseCode { get; }

        public DisconnectedEventArgs(
            string message = null,
            Exception exception = null,
            ResponseCode? errorCode = null)
        {
            DisconnectedAt = DateTime.UtcNow;
            Message = message;
            Exception = exception;
            ResponseCode = errorCode;
        }
    }

}