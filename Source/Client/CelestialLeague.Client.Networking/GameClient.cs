using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using CelestialLeague.Shared.Packets;
using Celeste.Mod;
using CelestialLeague.Shared.Enum;

namespace CelestialLeague.Client.Networking
{
    public class GameClient : IDisposable
    {
        private TcpClient _tcpClient;
        private NetworkStream _networkStream;
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        private bool _isConnected;
        private bool _isDisposed;

        private NetworkClient _networkClient;

        public ConnectionManager ConnectionManager;

        public bool IsConnected => _isConnected && !_isDisposed && ConnectionManager?.IsConnected == true;
        public string ServerEndpoint => ConnectionManager?.ServerEndpoint;
        public DateTime? ConnectedAt => ConnectionManager?.ConnectedAt;

        public event EventHandler<PacketReceivedEventArgs> OnPacketReceived;
        public event EventHandler<DisconnectedEventArgs> OnDisconnected;

        public GameClient()
        {
            _networkClient = new NetworkClient();
            ConnectionManager = new ConnectionManager(this, _networkClient);

            ConnectionManager.OnPacketReceived += (sender, args) => OnPacketReceived?.Invoke(this, args);
            ConnectionManager.OnDisconnected += (sender, args) =>
            {
                _isConnected = false;
                OnDisconnected?.Invoke(this, args);
            };
        }

        public async Task<bool> ConnectAsync(string host, int port, TimeSpan? timeout = null)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(GameClient));

            if (_isConnected)
                return true;

            try
            {
                _tcpClient = new TcpClient();

                var connectTimeout = timeout ?? TimeSpan.FromSeconds(10);
                using var timeoutCts = new CancellationTokenSource(connectTimeout);
                using var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(
                    _cancellationTokenSource.Token, timeoutCts.Token);

                await _tcpClient.ConnectAsync(host, port);

                if (!_tcpClient.Connected)
                {
                    Logger.Error("Celestial League", "Failed to connect to server");
                    return false;
                }

                _networkStream = _tcpClient.GetStream();

                var serverEndpoint = $"{host}:{port}";
                await ConnectionManager!.StartAsync(_networkStream, serverEndpoint);

                _isConnected = true;

                return true;
            }
            catch (SocketException ex)
            {
                Logger.Error("Celestial League", $"Socket error connecting to {host}:{port}: {ex.Message}");
                await CleanupConnectionAsync();
                return false;
            }
            catch (OperationCanceledException)
            {
                Logger.Error("Celestial League", $"Connection to {host}:{port} timed out");
                await CleanupConnectionAsync();
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error("Celestial League", $"Unexpected error connecting to {host}:{port}: {ex.Message}");
                await CleanupConnectionAsync();
                return false;
            }
        }

        public async Task DisconnectAsync(string reason = "Client disconnected")
        {
            if (!_isConnected)
                return;

            _isConnected = false;

            try
            {
                if (ConnectionManager != null)
                {
                    await ConnectionManager.StopAsync(reason);
                }
            }
            catch (Exception ex)
            {
                Logger.Warn("Celestial League", $"Error during disconnect: {ex.Message}");
            }
            finally
            {
                await CleanupConnectionAsync();
            }
        }

        public async Task<TResponse> SendRequestAsync<TRequest, TResponse>(
            TRequest request,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default)
            where TRequest : BasePacket
            where TResponse : BasePacket
        {
            if (!IsConnected || ConnectionManager == null)
                throw new InvalidOperationException("Not connected to server");

            return await ConnectionManager.SendRequestAsync<TRequest, TResponse>(request, timeout, cancellationToken);
        }

        public async Task<bool> SendPacketAsync<T>(T packet) where T : BasePacket
        {
            if (!IsConnected || ConnectionManager == null)
                return false;

            return await ConnectionManager.SendPacketAsync(packet);
        }

        public ConnectionQuality GetConnectionQuality()
        {
            return ConnectionManager?.GetConnectionQuality() ?? ConnectionQuality.VeryPoor;
        }

        public NetworkStatistics GetNetworkStatistics()
        {
            return ConnectionManager?.GetNetworkStatistics() ?? new NetworkStatistics();
        }

        private async Task CleanupConnectionAsync()
        {
            try
            {
                if (_networkStream != null)
                {
                    try
                    {
                        using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(1));
                        await _networkStream.FlushAsync(timeoutCts.Token);
                    }
                    catch
                    {
                        // continue with cleanup
                    }

                    _networkStream.Close();
                    _networkStream.Dispose();
                    _networkStream = null;
                }

                if (_tcpClient != null)
                {
                    _tcpClient.Close();
                    _tcpClient.Dispose();
                    _tcpClient = null;
                }
            }
            catch (Exception ex)
            {
                Logger.Warn("Celestial League", $"Error during connection cleanup: {ex.Message}");
            }
        }


        public void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;

            try
            {
                DisconnectAsync("Client disposed").Wait(TimeSpan.FromSeconds(2));

                _cancellationTokenSource?.Cancel();

                ConnectionManager?.Dispose();
                _networkClient?.Dispose();

                CleanupConnectionAsync().Wait(TimeSpan.FromSeconds(1));

                _cancellationTokenSource?.Dispose();
            }
            catch (Exception ex)
            {
                Logger.Error("Celestial League", $"Error during GameClient disposal: {ex.Message}");
            }
        }
    }
}

