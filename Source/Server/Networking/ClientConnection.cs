using System.Net;
using System.Net.Sockets;
using CelestialLeague.Server.Core;
using CelestialLeague.Server.Models;
using CelestialLeague.Server.Utils;
using CelestialLeague.Shared.Enums;
using CelestialLeague.Shared.Packets;
using CelestialLeague.Shared.Utils;

namespace CelestialLeague.Server.Networking
{
    public class ClientConnection : IDisposable
    {
        // private fields
        private readonly TcpClient _tcpClient;
        private readonly NetworkStream? _stream;
        private readonly GameServer _gameServer;
        private Logger _logger => _gameServer.Logger;
        private PacketProcessor _packetProcessor => _gameServer.PacketProcessor;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly object _sendLock = new object();
        private Task? _receiveTask;
        private bool _isDisposed;

        // statistics
        public int PacketsSent { get; private set; }
        public int PacketsReceived { get; private set; }
        public int SendErrors { get; private set; }
        public int ReceiveErrors { get; private set; }
        public int DeserializationErrors { get; private set; }
        public int ProcessingErrors { get; private set; }

        // public properties
        public Session? Session { get; private set; }
        public bool IsAuthenticated => Session != null;
        public string ConnectionID { get; private set; } = Guid.NewGuid().ToString("N");
        public IPEndPoint? RemoteEndpoint => _tcpClient.Client.RemoteEndPoint as IPEndPoint;
        public bool IsConnected => _tcpClient.Connected && !_isDisposed;
        public DateTime ConnectedAt { get; private set; } = DateTime.UtcNow;
        public DateTime LastActivity { get; private set; } = DateTime.UtcNow;
        public ConnectionQuality ConnectionQuality => CalculateConnectionQuality();

        // events
        public event EventHandler<PacketReceivedEventArgs>? OnPacketReceived;
        public event EventHandler<DisconnectedEventArgs>? OnDisconnected;

        public ClientConnection(GameServer gameServer, TcpClient tcpClient)
        {
            _tcpClient = tcpClient ?? throw new ArgumentNullException(nameof(tcpClient));
            _gameServer = gameServer ?? throw new ArgumentNullException(nameof(gameServer));
            _cancellationTokenSource = new CancellationTokenSource();
            _stream = tcpClient.GetStream();
        }

        public async Task StartAsync()
        {
            ThrowIfDisposed();
            if (!IsConnected)
                throw new InvalidOperationException("Not connected");

            try
            {
                _logger.Info($"Starting connection handler for {RemoteEndpoint} (ID: {ConnectionID})");
                _receiveTask = ReceivePacketsAsync(_cancellationTokenSource.Token);
                await _receiveTask.ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.Error($"Error in connection handler for {ConnectionID}: {ex.Message}");
                throw;
            }
        }

        public async Task DisconnectAsync(string reason = "Server requested disconnect")
        {
            if (_isDisposed || !IsConnected)
                return;

            try
            {
                _logger.Info($"Disconnecting {ConnectionID}: {reason}");

                await _cancellationTokenSource.CancelAsync().ConfigureAwait(false);

                if (_receiveTask != null)
                {
                    await _receiveTask.ConfigureAwait(false);
                }

                _stream?.Close();
                _tcpClient.Close();

                OnDisconnected?.Invoke(this, new DisconnectedEventArgs(reason));
                _logger.Info($"Disconnected {ConnectionID} from {RemoteEndpoint}");
            }
            catch (Exception ex)
            {
                _logger.Error($"Error disconnecting {ConnectionID}: {ex.Message}");
            }
        }

        public async Task<bool> SendPacketAsync<T>(T packet) where T : BasePacket
        {
            ThrowIfDisposed();

            if (!IsConnected || _stream == null)
            {
                _logger.Warning($"Cannot send {typeof(T).Name} to {ConnectionID}: Not connected");
                return false;
            }

            try
            {
                ArgumentNullException.ThrowIfNull(packet, nameof(packet));
                if (!packet.IsValid())
                {
                    _logger.Warning($"Attempted to send invalid {packet.Type} to {ConnectionID}");
                    return false;
                }

                var data = Serialization.SerializePacket(packet);

                if (data.Length > NetworkConstants.MaxPacketSize)
                {
                    _logger.Warning($"Packet too large for {ConnectionID}: {data.Length} bytes");
                    SendErrors++;
                    return false;
                }

                lock (_sendLock)
                {
                    var lengthBytes = BitConverter.GetBytes(data.Length);
                    _stream.WriteAsync(lengthBytes, 0, 4);

                    _stream.WriteAsync(data, 0, data.Length);
                    _stream.FlushAsync();
                }

                PacketsSent++;
                UpdateActivity();

                _logger.Debug($"Sent {packet.Type} to {ConnectionID} ({data.Length} bytes)");
                return true;
            }
            catch (Exception ex)
            {
                SendErrors++;
                _logger.Error($"Error sending {typeof(T).Name} to {ConnectionID}: {ex.Message}");
                await DisconnectAsync($"Send error: {ex.Message}").ConfigureAwait(false);
                return false;
            }
        }

        private async Task ReceivePacketsAsync(CancellationToken cancellationToken)
        {
            var buffer = new byte[NetworkConstants.MaxPacketSize];

            while (IsConnected && !cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var lengthBytes = new byte[4];
                    var bytesRead = await ReadExactAsync(_stream!, lengthBytes, 4, cancellationToken).ConfigureAwait(false);

                    if (bytesRead != 4)
                    {
                        _logger.Warning($"Failed to read packet length header from {ConnectionID}");
                        break;
                    }

                    var packetLength = BitConverter.ToInt32(lengthBytes, 0);

                    if (packetLength <= 0 || packetLength > NetworkConstants.MaxPacketSize)
                    {
                        _logger.Error($"Invalid packet length from {ConnectionID}: {packetLength}");
                        ReceiveErrors++;
                        break;
                    }

                    var packetData = new byte[packetLength];
                    bytesRead = await ReadExactAsync(_stream!, packetData, packetLength, cancellationToken).ConfigureAwait(false);

                    if (bytesRead != packetLength)
                    {
                        _logger.Warning($"Incomplete packet from {ConnectionID}: {bytesRead}/{packetLength} bytes");
                        ReceiveErrors++;
                        break;
                    }

                    await ProcessReceivedDataAsync(packetData, packetLength).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    ReceiveErrors++;
                    _logger.Error($"Error receiving data from {ConnectionID}: {ex.Message}");
                    break;
                }
            }
        }

        private static async Task<int> ReadExactAsync(NetworkStream stream, byte[] buffer, int count, CancellationToken cancellationToken)
        {
            int totalBytesRead = 0;

            while (totalBytesRead < count && !cancellationToken.IsCancellationRequested)
            {
                var bytesRead = await stream.ReadAsync(buffer.AsMemory(totalBytesRead, count - totalBytesRead), cancellationToken).ConfigureAwait(false);

                if (bytesRead == 0)
                    break; // connection closed

                totalBytesRead += bytesRead;
            }

            return totalBytesRead;
        }

        private async Task ProcessReceivedDataAsync(byte[] data, int length)
        {
            ThrowIfDisposed();

            try
            {
                var packet = Serialization.DeserializePacket(data, length);
                _logger.Info(packet?.ToString()!);
                if (packet != null)
                {
                    PacketsReceived++;
                    UpdateActivity();

                    OnPacketReceived?.Invoke(this, new PacketReceivedEventArgs(packet, length));
                    _logger.Info($"Received {packet.Type} from {ConnectionID} ({length} bytes)");

                    await _packetProcessor.ProcessAsync(this, packet).ConfigureAwait(false);
                }
                else
                {
                    DeserializationErrors++;
                    _logger.Warning($"Failed to deserialize packet from {ConnectionID} ({length} bytes)");

                    if (DeserializationErrors > NetworkConstants.MaxDeserializationErrors)
                    {
                        await DisconnectAsync("Too many invalid packets").ConfigureAwait(false);
                    }
                }
            }
            catch (Exception ex)
            {
                ProcessingErrors++;
                _logger.Error($"Error processing packet from {ConnectionID}: {ex.Message}");

                if (ex is OutOfMemoryException || ex is StackOverflowException)
                {
                    await DisconnectAsync($"Critical error: {ex.GetType().Name}").ConfigureAwait(false);
                }
            }
        }

        private ConnectionQuality CalculateConnectionQuality()
        {
            var totalPackets = PacketsSent + PacketsReceived;
            if (totalPackets == 0) return ConnectionQuality.VeryPoor;

            var totalErrors = SendErrors + ReceiveErrors + DeserializationErrors;
            var errorRate = (double)totalErrors / totalPackets;
            var timeSinceLastActivity = DateTime.UtcNow - LastActivity;

            if (timeSinceLastActivity > TimeSpan.FromSeconds(30))
                return ConnectionQuality.Poor;
            else if (errorRate > 0.1)
                return ConnectionQuality.Poor;
            else if (errorRate > 0.05)
                return ConnectionQuality.Fair;
            else if (errorRate > 0.01)
                return ConnectionQuality.Good;
            else
                return ConnectionQuality.Excellent;
        }

        public void UpdateActivity()
        {
            LastActivity = DateTime.UtcNow;
        }

        public void SetSession(Session session)
        {
            Session = session ?? throw new ArgumentNullException(nameof(session));
            UpdateActivity();
        }

        public void ClearSession()
        {
            Session = null;
            UpdateActivity();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed)
                return;

            if (disposing)
            {
                try
                {
                    DisconnectAsync("Connection disposed").Wait(1000);
                }
                catch
                {
                    // ignore exceptions during dispose
                }

                _cancellationTokenSource?.Dispose();
                _stream?.Dispose();
                _tcpClient?.Dispose();
            }

            _isDisposed = true;
        }

        private void ThrowIfDisposed()
        {
            ObjectDisposedException.ThrowIf(_isDisposed, nameof(ClientConnection));
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
        public string? Message { get; }
        public Exception? Exception { get; }
        public DateTime DisconnectedAt { get; }
        public ResponseErrorCode? ErrorCode { get; }

        public DisconnectedEventArgs(
            string? message = null,
            Exception? exception = null,
            ResponseErrorCode? errorCode = null)
        {
            DisconnectedAt = DateTime.UtcNow;
            Message = message;
            Exception = exception;
            ErrorCode = errorCode;
        }
    }
}