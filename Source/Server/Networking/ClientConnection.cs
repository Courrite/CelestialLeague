using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;
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
        private TcpClient _tcpClient { get; set; }
        private NetworkStream? _stream { get; set; }
        private GameServer _gameServer { get; set; }
        private Logger _logger => _gameServer.Logger;
        private PacketProcessor _packetProcessor { get; set; }
        private CancellationTokenSource? _cancellationTokenSource { get; set; }
        // public SemaphoreSlim _semaphore { get; set; } // overkill for now
        private Task? _receiveTask;
        private bool _isDisposed { get; set; }

        // public properties
        public Session? Session { get; private set; }
        public bool IsAuthenticated => Session != null;
        public string ConnectionID { get; private set; } = Guid.NewGuid().ToString("N");
        public IPEndPoint? RemoteEndpoint => _tcpClient.Client.RemoteEndPoint as IPEndPoint;
        public bool IsConnected => _tcpClient.Connected == true && !_isDisposed;
        public DateTime LastActivity { get; private set; } = DateTime.UtcNow;

        // events
        public event EventHandler<PacketReceivedEventArgs>? OnPacketReceived;
        public event EventHandler<DisconnectedEventArgs>? OnDisconnected;

        public ClientConnection(GameServer gameServer, TcpClient tcpClient)
        {
            _tcpClient = tcpClient;
            _gameServer = gameServer;
            _packetProcessor = new PacketProcessor(_gameServer);
            _cancellationTokenSource = new CancellationTokenSource();
            // _semaphore = new SemaphoreSlim(1, 1);
            _stream = tcpClient?.GetStream();
        }

        public async Task StartAsync()
        {
            ThrowIfDisposed();
            if (!IsConnected)
                throw new InvalidOperationException("Not connected");

            try
            {
                _logger.Info($"start connection handler for {RemoteEndpoint}");
                _receiveTask = ReceivePacketsAsync(_cancellationTokenSource!.Token);
                await _receiveTask.ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.Error($"error in connection handler: {ex.Message}");
                throw;
            }
        }

        public async Task DisconnectAsync()
        {
            ThrowIfDisposed();
            if (!IsConnected)
                return;

            try
            {
                _cancellationTokenSource!.Cancel();
                if (_receiveTask != null)
                    await _receiveTask.ConfigureAwait(false);
                _stream?.Close();
                _tcpClient.Close();
                _logger.Info($"disconnected from {_tcpClient.Client.RemoteEndPoint}");
                OnDisconnected?.Invoke(this, new DisconnectedEventArgs());
            }
            catch (Exception ex)
            {
                _logger.Error($"error disconnecting from server: {ex.Message}");
            }
        }

        public async Task SendPacketAsync<T>(T packet) where T : BasePacket
        {
            ThrowIfDisposed();
            if (!IsConnected)
                throw new InvalidOperationException("Not connected");

            if (_stream == null)
                throw new InvalidOperationException("Network stream is null");

            try
            {
                if (!packet.IsValid())
                {
                    _logger.Warning($"attempted to send invalid packet: {packet.Type}");
                    return;
                }

                var json = Serialization.ToJson(packet);
                var data = Encoding.UTF8.GetBytes(json);

                if (data.Length > NetworkConstants.MaxPacketSize)
                {
                    _logger.Warning($"packet too large: {data.Length} bytes");
                    return;
                }

                await _stream.WriteAsync(data).ConfigureAwait(false);
                await _stream.FlushAsync().ConfigureAwait(false);

                _logger.Debug($"sent {packet.Type} packet to {ConnectionID} ({data.Length} bytes)");
            }
            catch (Exception ex)
            {
                _logger.Error($"error sending packet to {RemoteEndpoint}: {ex.Message}");
            }
        }

        private async Task ReceivePacketsAsync(CancellationToken cancellationToken)
        {
            var buffer = new byte[1024 * 4];
            while (IsConnected && !cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var bytesRead = await _stream!.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
                    if (bytesRead == 0)
                        break;

                    await ProcessReceivedDataAsync(buffer.AsMemory(0, bytesRead)).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.Error($"error receiving data: {ex.Message}");
                    break;
                }
            }
        }

        private async Task ProcessReceivedDataAsync(ReadOnlyMemory<byte> data)
        {
            ThrowIfDisposed();
            try
            {
                var json = Encoding.UTF8.GetString(data.Span);
                var packet = Serialization.FromJson<BasePacket>(json);

                if (packet != null)
                {
                    OnPacketReceived?.Invoke(this, new PacketReceivedEventArgs(packet, data.Length));
                    _logger.Info($"received packet: {packet.GetType().Name}");

                    await _packetProcessor.ProcessAsync(this, packet).ConfigureAwait(false);
                }
                else
                {
                    _logger.Warning($"failed to deserialize packet from {ConnectionID}");
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"error processing received data: {ex.Message}");
            }
        }

        public void UpdateActivity()
        {
            LastActivity = DateTime.UtcNow;
        }

        public void SetSession(Session session)
        {
            Session = session;
        }

        public void ClearSession()
        {
            Session = null;
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
                _cancellationTokenSource?.Cancel();
                _cancellationTokenSource?.Dispose();
                _stream?.Dispose();
                _tcpClient?.Dispose();
                // _semaphore?.Dispose();
            }
            _isDisposed = true;
        }

        private void ThrowIfDisposed()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(ClientConnection), "ClientConnection has been disposed");
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
            Packet = packet;
            PacketType = packet.Type;
            ReceivedAt = DateTime.UtcNow;
            PacketSize = packetSize;
            CorrelationId = packet.CorrelationId;
        }

        public bool IsPacketType<T>() where T : BasePacket
        {
            return Packet is T;
        }

        public bool IsPacketType(PacketType packetType)
        {
            return Packet.Type == packetType;
        }
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
            ResponseErrorCode? errorCode = null
        )
        {
            DisconnectedAt = DateTime.UtcNow;
            Message = message;
            Exception = exception;
            ErrorCode = errorCode;
        }
    }
}