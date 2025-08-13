using System.Net;
using System.Net.Sockets;
using CelestialLeague.Server.Models;
using CelestialLeague.Server.Utils;
using CelestialLeague.Shared.Enum;
using CelestialLeague.Shared.Packets;
using CelestialLeague.Shared.Utils;

namespace CelestialLeague.Server.Networking
{
    public sealed class ClientConnection : IDisposable
    {
        private readonly TcpClient _tcpClient;
        private readonly NetworkStream _stream;
        private readonly GameServer _gameServer;
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        private readonly SemaphoreSlim _sendSemaphore = new(1, 1);
        private Task? _receiveTask;
        private bool _disposed;

        // Statistics
        public int PacketsSent { get; private set; }
        public int PacketsReceived { get; private set; }
        public int SendErrors { get; private set; }
        public int ReceiveErrors { get; private set; }
        public int DeserializationErrors { get; private set; }

        // Properties
        public Session? Session { get; private set; }
        public bool IsAuthenticated => Session != null;
        public string ConnectionID { get; } = Guid.NewGuid().ToString("N");
        public IPEndPoint? RemoteEndpoint => _tcpClient.Client.RemoteEndPoint as IPEndPoint;
        public bool IsConnected => _tcpClient.Connected && !_disposed;
        public DateTime ConnectedAt { get; } = DateTime.UtcNow;
        public DateTime LastActivity { get; private set; } = DateTime.UtcNow;
        public ConnectionQuality ConnectionQuality => CalculateConnectionQuality();

        // Events
        public event EventHandler<PacketReceivedEventArgs>? OnPacketReceived;
        public event EventHandler<DisconnectedEventArgs>? OnDisconnected;

        private Logger Logger => _gameServer.Logger;
        private PacketProcessor PacketProcessor => _gameServer.PacketProcessor;

        public ClientConnection(GameServer gameServer, TcpClient tcpClient)
        {
            ArgumentNullException.ThrowIfNull(tcpClient);

            _gameServer = gameServer;
            _tcpClient = tcpClient;
            _stream = tcpClient.GetStream();
        }

        public async Task StartAsync()
        {
            if (_disposed || !IsConnected)
                throw new InvalidOperationException("Connection not available");

            Logger.Info($"Starting connection handler for {RemoteEndpoint} (ID: {ConnectionID})");
            _receiveTask = ReceivePacketsAsync(_cancellationTokenSource.Token);
            await _receiveTask.ConfigureAwait(false);
        }

        public async Task DisconnectAsync(string reason = "Server disconnect", ResponseCode? errorCode = null)
        {
            if (_disposed || !IsConnected) return;

            Logger.Info($"Disconnecting {ConnectionID}: {reason}");

            await SendDisconnectPacketAsync(reason, errorCode).ConfigureAwait(false);

            await _cancellationTokenSource.CancelAsync().ConfigureAwait(false);
            if (_receiveTask != null)
            {
                try
                {
                    await _receiveTask.WaitAsync(TimeSpan.FromSeconds(2)).ConfigureAwait(false);
                }
                catch (TimeoutException)
                {
                    // task didn't complete gracefully, continue with cleanup
                }
            }

            _stream.Close();
            _tcpClient.Close();

            var disconnectArgs = new DisconnectedEventArgs(reason, errorCode: errorCode);
            OnDisconnected?.Invoke(this, disconnectArgs);
            Logger.Info($"Disconnected {ConnectionID}");
        }

        private async Task SendDisconnectPacketAsync(string reason, ResponseCode? errorCode)
        {
            try
            {
                var reconnect = errorCode switch
                {
                    ResponseCode.NETWORK_MAINTENANCE => true,
                    ResponseCode.NETWORK_RATE_LIMITED => true,
                    ResponseCode.NETWORK_TIMEOUT => true,
                    ResponseCode.FORBIDDEN => false,
                    ResponseCode.NETWORK_INVALID_PACKET => false,
                    _ => false
                };

                var disconnectPacket = new DisconnectPacket(
                    errorCode ?? ResponseCode.SUCCESS,
                    reason,
                    reconnect
                );

                await SendPacketAsync(disconnectPacket).ConfigureAwait(false);
                await Task.Delay(100).ConfigureAwait(false);
            }
            catch
            {
                // if disconnect packet fails to send, continue with disconnect
            }
        }

        public async Task<bool> SendPacketAsync<T>(T packet) where T : BasePacket
        {
            if (_disposed || !IsConnected || packet == null)
                return false;

            if (!packet.IsValid())
            {
                Logger.Warning($"Invalid {packet.Type} packet for {ConnectionID}");
                return false;
            }

            // use semaphore to ensure ordered, atomic writes
            await _sendSemaphore.WaitAsync(_cancellationTokenSource.Token).ConfigureAwait(false);
            try
            {
                var data = Serialization.SerializePacket(packet);
                if (data.Length > Shared.Constants.Network.MaxPacketSize)
                {
                    SendErrors++;
                    Logger.Warning($"Packet too large: {data.Length} bytes");
                    return false;
                }

                // write length header + data atomically
                var lengthBytes = BitConverter.GetBytes(data.Length);
                await _stream.WriteAsync(lengthBytes.AsMemory(0, 4), _cancellationTokenSource.Token).ConfigureAwait(false);
                await _stream.WriteAsync(data, _cancellationTokenSource.Token).ConfigureAwait(false);
                await _stream.FlushAsync(_cancellationTokenSource.Token).ConfigureAwait(false);

                PacketsSent++;
                LastActivity = DateTime.UtcNow;
                Logger.Debug($"Sent {packet.Type} to {ConnectionID}");
                return true;
            }
            catch (OperationCanceledException)
            {
                return false;
            }
            catch (Exception ex)
            {
                SendErrors++;
                Logger.Error($"Send error for {ConnectionID}: {ex.Message}");
                _ = DisconnectAsync($"Send failed: {ex.Message}", ResponseCode.INTERNAL_ERROR);
                return false;
            }
            finally
            {
                _sendSemaphore.Release();
            }
        }

        private async Task ReceivePacketsAsync(CancellationToken cancellationToken)
        {
            while (IsConnected && !cancellationToken.IsCancellationRequested)
            {
                try
                {
                    // read packet length header
                    var lengthBytes = new byte[4];
                    var headerRead = await ReadExactAsync(_stream, lengthBytes, 4, cancellationToken).ConfigureAwait(false);
                    if (headerRead != 4) break;

                    var packetLength = BitConverter.ToInt32(lengthBytes, 0);
                    if (packetLength <= 0 || packetLength > Shared.Constants.Network.MaxPacketSize)
                    {
                        ReceiveErrors++;
                        Logger.Error($"Invalid packet length: {packetLength}");
                        break;
                    }

                    // read packet data
                    var packetData = new byte[packetLength];
                    var dataRead = await ReadExactAsync(_stream, packetData, packetLength, cancellationToken).ConfigureAwait(false);
                    if (dataRead != packetLength)
                    {
                        ReceiveErrors++;
                        Logger.Warning($"Incomplete packet: {dataRead}/{packetLength} bytes");
                        break;
                    }

                    await ProcessReceivedDataAsync(packetData).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    ReceiveErrors++;
                    Logger.Error($"Receive error: {ex.Message}");
                    break;
                }
            }
        }

        private static async Task<int> ReadExactAsync(NetworkStream stream, byte[] buffer, int count, CancellationToken cancellationToken)
        {
            int totalRead = 0;
            while (totalRead < count && !cancellationToken.IsCancellationRequested)
            {
                var read = await stream.ReadAsync(buffer.AsMemory(totalRead, count - totalRead), cancellationToken).ConfigureAwait(false);
                if (read == 0) break; // connection closed
                totalRead += read;
            }
            return totalRead;
        }

        private async Task ProcessReceivedDataAsync(byte[] data)
        {
            if (_disposed) return;

            var packet = Serialization.DeserializePacket(data, data.Length);
            if (packet == null)
            {
                DeserializationErrors++;
                if (DeserializationErrors > Shared.Constants.Network.MaxDeserializationErrors)
                {
                    await DisconnectAsync("Too many bad packets", ResponseCode.NETWORK_INVALID_PACKET).ConfigureAwait(false);
                }
                return;
            }

            PacketsReceived++;
            LastActivity = DateTime.UtcNow;

            OnPacketReceived?.Invoke(this, new PacketReceivedEventArgs(packet, data.Length));
            Logger.Debug($"Received {packet.Type} from {ConnectionID}");

            await PacketProcessor.ProcessAsync(this, packet).ConfigureAwait(false);
        }

        private ConnectionQuality CalculateConnectionQuality()
        {
            var totalPackets = PacketsSent + PacketsReceived;
            if (totalPackets == 0) return ConnectionQuality.VeryPoor;

            var totalErrors = SendErrors + ReceiveErrors + DeserializationErrors;
            var errorRate = (double)totalErrors / totalPackets;
            var idleTime = DateTime.UtcNow - LastActivity;

            return idleTime.TotalSeconds > 30 ? ConnectionQuality.Poor :
                   errorRate > 0.1 ? ConnectionQuality.Poor :
                   errorRate > 0.05 ? ConnectionQuality.Fair :
                   errorRate > 0.01 ? ConnectionQuality.Good :
                   ConnectionQuality.Excellent;
        }

        public void SetSession(Session session)
        {
            Session = session;
            LastActivity = DateTime.UtcNow;
        }

        public void ClearSession()
        {
            Session = null;
            LastActivity = DateTime.UtcNow;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            try
            {
                _cancellationTokenSource.Cancel();
                _stream?.Close();
                _tcpClient?.Close();
                Logger?.Info($"Connection {ConnectionID} disposed");
            }
            catch (Exception ex)
            {
                Logger?.Debug($"Dispose cleanup warning: {ex.Message}");
            }
            finally
            {
                _sendSemaphore?.Dispose();
                _cancellationTokenSource?.Dispose();
                _stream?.Dispose();
                _tcpClient?.Dispose();
                GC.SuppressFinalize(this);
            }
        }
    }

    public class PacketReceivedEventArgs : EventArgs
    {
        public BasePacket Packet { get; }
        public PacketType PacketType => Packet.Type;
        public DateTime ReceivedAt { get; } = DateTime.UtcNow;
        public int PacketSize { get; }
        public uint? CorrelationId => Packet.CorrelationId;

        public PacketReceivedEventArgs(BasePacket packet, int packetSize)
        {
            Packet = packet;
            PacketSize = packetSize;
        }

        public bool IsPacketType<T>() where T : BasePacket => Packet is T;
        public bool IsPacketType(PacketType packetType) => PacketType == packetType;
    }

    public class DisconnectedEventArgs : EventArgs
    {
        public string? Message { get; }
        public Exception? Exception { get; }
        public ResponseCode? ResponseCode { get; }
        public DateTime DisconnectedAt { get; } = DateTime.UtcNow;

        public DisconnectedEventArgs(string? message = null, Exception? exception = null, ResponseCode? errorCode = null)
        {
            Message = message;
            Exception = exception;
            ResponseCode = errorCode;
        }

        public override string ToString()
        {
            var errorInfo = ResponseCode.HasValue ? $" ({ResponseCode})" : "";
            return $"Disconnected{errorInfo}: {Message ?? "No reason provided"}";
        }
    }
}
//