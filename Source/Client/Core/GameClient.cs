using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using CelestialLeague.Client.Networking;
using CelestialLeague.Shared.Packets;
using Celeste.Mod;
using CelestialLeague.Shared.Enums;

namespace CelestialLeague.Client.Core
{
    public class GameClient : IDisposable
    {
        private TcpClient _tcpClient;
        private NetworkStream _networkStream;
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        private bool _isConnected;
        private bool _isDisposed;

        private NetworkClient _networkClient;
        private ConnectionManager _connectionManager;

        public bool IsConnected => _isConnected && !_isDisposed && _connectionManager?.IsConnected == true;
        public string ServerEndpoint => _connectionManager?.ServerEndpoint;
        public DateTime? ConnectedAt => _connectionManager?.ConnectedAt;

        public event EventHandler<PacketReceivedEventArgs> OnPacketReceived;
        public event EventHandler<DisconnectedEventArgs> OnDisconnected;

        public GameClient()
        {
            _networkClient = new NetworkClient();
            _connectionManager = new ConnectionManager(this, _networkClient);

            _connectionManager.OnPacketReceived += (sender, args) => OnPacketReceived?.Invoke(this, args);
            _connectionManager.OnDisconnected += (sender, args) =>
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
                Logger.Info("CelestialLeague", $"Connecting to {host}:{port}...");

                _tcpClient = new TcpClient();

                var connectTimeout = timeout ?? TimeSpan.FromSeconds(10);
                using var timeoutCts = new CancellationTokenSource(connectTimeout);
                using var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(
                    _cancellationTokenSource.Token, timeoutCts.Token);

                await _tcpClient.ConnectAsync(host, port);

                if (!_tcpClient.Connected)
                {
                    Logger.Error("CelestialLeague", "Failed to connect to server");
                    return false;
                }

                _networkStream = _tcpClient.GetStream();

                var serverEndpoint = $"{host}:{port}";
                await _connectionManager!.StartAsync(_networkStream, serverEndpoint);

                _isConnected = true;
                Logger.Info("CelestialLeague", $"Connected to {serverEndpoint}");

                return true;
            }
            catch (SocketException ex)
            {
                Logger.Error("CelestialLeague", $"Socket error connecting to {host}:{port}: {ex.Message}");
                await CleanupConnectionAsync();
                return false;
            }
            catch (OperationCanceledException)
            {
                Logger.Error("CelestialLeague", $"Connection to {host}:{port} timed out");
                await CleanupConnectionAsync();
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error("CelestialLeague", $"Unexpected error connecting to {host}:{port}: {ex.Message}");
                await CleanupConnectionAsync();
                return false;
            }
        }

        public async Task DisconnectAsync(string reason = "Client disconnected")
        {
            if (!_isConnected)
                return;

            Logger.Info("CelestialLeague", $"Disconnecting: {reason}");

            _isConnected = false;

            try
            {
                if (_connectionManager != null)
                {
                    await _connectionManager.StopAsync(reason);
                }
            }
            catch (Exception ex)
            {
                Logger.Warn("CelestialLeague", $"Error during disconnect: {ex.Message}");
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
            if (!IsConnected || _connectionManager == null)
                throw new InvalidOperationException("Not connected to server");

            return await _connectionManager.SendRequestAsync<TRequest, TResponse>(request, timeout, cancellationToken);
        }

        public async Task<bool> SendPacketAsync<T>(T packet) where T : BasePacket
        {
            if (!IsConnected || _connectionManager == null)
                return false;

            return await _connectionManager.SendPacketAsync(packet);
        }

        public ConnectionQuality GetConnectionQuality()
        {
            return _connectionManager?.GetConnectionQuality() ?? ConnectionQuality.VeryPoor;
        }

        public NetworkStatistics GetNetworkStatistics()
        {
            return _connectionManager?.GetNetworkStatistics() ?? new NetworkStatistics();
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
                Logger.Warn("CelestialLeague", $"Error during connection cleanup: {ex.Message}");
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

                _connectionManager?.Dispose();
                _networkClient?.Dispose();

                CleanupConnectionAsync().Wait(TimeSpan.FromSeconds(1));

                _cancellationTokenSource?.Dispose();

                Logger.Info("CelestialLeague", "GameClient disposed");
            }
            catch (Exception ex)
            {
                Logger.Error("CelestialLeague", $"Error during GameClient disposal: {ex.Message}");
            }
        }
    }
}
