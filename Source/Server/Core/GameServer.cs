using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Sockets;
using CelestialLeague.Server.Database.Context;
using CelestialLeague.Server.Models;
using CelestialLeague.Server.Networking;
using CelestialLeague.Server.Services;
using CelestialLeague.Server.Utils;

namespace CelestialLeague.Server.Core
{
    public class GameServer : IDisposable
    {
        public Logger Logger { get; private set; }
        public IPAddress IPAddress { get; private set; }
        public int Port { get; private set; }
        public SessionManager SessionManager { get; private set; }
        public GameDbContext GameDbContext { get; private set; }
        public AuthenticationService AuthenticationService { get; private set; }
        public PacketProcessor PacketProcessor { get; private set; }

        private TcpListener? _tcpListener;
        private readonly CancellationTokenSource? _cancellationTokenSource;
        private Task? _acceptTask;

        private readonly ConcurrentDictionary<string, ClientConnection> _connections = new();

        private bool _isRunning;
        private bool _isDisposed;

        public bool IsRunning => _isRunning && !_isDisposed;
        public int ConnectedClients => _connections.Count;

        public Task<int> ActiveSessions => SessionManager.GetActiveSessionCountAsync();

        public GameServer(IPAddress ipAddress, int port, Logger logger, string? connectionString = null)
        {
            IPAddress = ipAddress;
            Port = port;
            Logger = logger ?? new Logger();
            SessionManager = new SessionManager();
            GameDbContext = new GameDbContextFactory().CreateDbContext([connectionString ?? "Data Source=celestial_league.db"]);
            AuthenticationService = new AuthenticationService(this);
            PacketProcessor = new PacketProcessor(this, HandlerFactory.CreateAllHandlers(this));

            _cancellationTokenSource = new CancellationTokenSource();
        }

        public void StartAsync()
        {
            ThrowIfDisposed();
            if (_isRunning)
                throw new InvalidOperationException("Server is already running");

            try
            {
                _tcpListener = new TcpListener(IPAddress, Port);
                _tcpListener.Start();
                _isRunning = true;

                Logger.Info($"Server started on {IPAddress}:{Port}");

                _acceptTask = AcceptClientsAsync(_cancellationTokenSource!.Token);

                Logger.Info("Server is ready to accept connections");
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to start server: {ex.Message}");
                throw;
            }
        }

        public async Task StopAsync()
        {
            if (!_isRunning || _isDisposed)
                return;

            try
            {
                Logger.Info("Stopping server...");

                _isRunning = false;
                _cancellationTokenSource?.CancelAsync();

                _tcpListener?.Stop();

                if (_acceptTask != null)
                {
                    try
                    {
                        await _acceptTask.ConfigureAwait(false);
                    }
                    catch (OperationCanceledException)
                    {
                        // expected when cancelling
                    }
                }

                await DisconnectAllClientsAsync().ConfigureAwait(false);

                Logger.Info("Server stopped");
            }
            catch (Exception ex)
            {
                Logger.Error($"Error stopping server: {ex.Message}");
            }
        }

        private async Task AcceptClientsAsync(CancellationToken cancellationToken)
        {
            while (_isRunning && !cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var tcpClient = await _tcpListener!.AcceptTcpClientAsync(cancellationToken).ConfigureAwait(false);

                    if (cancellationToken.IsCancellationRequested)
                    {
                        tcpClient.Close();
                        break;
                    }

                    _ = Task.Run(async () => await HandleClientAsync(tcpClient).ConfigureAwait(false), cancellationToken);
                }
                catch (ObjectDisposedException)
                {
                    // expected when stopping server
                    break;
                }
                catch (Exception ex)
                {
                    if (_isRunning)
                    {
                        Logger.Error($"Error accepting client: {ex.Message}");
                        await Task.Delay(1000, cancellationToken).ConfigureAwait(false);
                    }
                }
            }
        }

        private async Task HandleClientAsync(TcpClient tcpClient)
        {
            ClientConnection? connection = null;
            try
            {
                connection = new ClientConnection(this, tcpClient);
                _connections.TryAdd(connection.ConnectionID, connection);

                Logger.Info($"Client connected: {connection.ConnectionID} from {connection.RemoteEndpoint}");

                connection.OnDisconnected += async (sender, e) =>
                {
                    Logger.Info($"Client disconnected: {connection.ConnectionID}");
                    await SessionManager.DisconnectAsync(connection.ConnectionID).ConfigureAwait(false);
                };

                await connection.StartAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Logger.Error($"Error handling client: {ex.Message}");
            }
            finally
            {
                if (connection != null)
                {
                    _connections.TryRemove(connection.ConnectionID, out _);
                    await SessionManager.DisconnectAsync(connection.ConnectionID).ConfigureAwait(false);

                    connection.Dispose();
                }

                tcpClient?.Close();
            }
        }

        private async Task DisconnectAllClientsAsync()
        {
            var disconnectTasks = new List<Task>();

            foreach (var connection in _connections.Values)
            {
                disconnectTasks.Add(connection.DisconnectAsync());
            }

            if (disconnectTasks.Count > 0)
            {
                await Task.WhenAll(disconnectTasks).ConfigureAwait(false);
            }

            _connections.Clear();

            // note: sessionmanager handles its own cleanup via timer
        }

        public async Task<string> CreateSessionAsync(int playerId) => await SessionManager.CreateSessionAsync(playerId).ConfigureAwait(false);
        public async Task<Session?> GetSessionAsync(string sessionToken)
        {
            if (await SessionManager.IsSessionValidAsync(sessionToken).ConfigureAwait(false))
            {
                var playerId = await SessionManager.GetPlayerIdAsync(sessionToken).ConfigureAwait(false);
                if (playerId != null)
                {
                    var sessions = await SessionManager.GetActiveSessionsForPlayerAsync(playerId.Value).ConfigureAwait(false);
                    return sessions.FirstOrDefault(s => s.SessionToken == sessionToken);
                }
            }
            return null;
        }

        public async Task<bool> IsSessionValidAsync(string sessionToken) => await SessionManager.IsSessionValidAsync(sessionToken).ConfigureAwait(false);
        public async Task InvalidateSessionAsync(string sessionToken) => await SessionManager.InvalidateSessionAsync(sessionToken).ConfigureAwait(false);
        public async Task<bool> SetConnectionAsync(string sessionToken, string connectionId) => await SessionManager.SetConnectionAsync(sessionToken, connectionId).ConfigureAwait(false);
        public async Task<Session?> GetSessionByConnectionAsync(string connectionId) => await SessionManager.GetSessionByConnectionAsync(connectionId).ConfigureAwait(false);
        public async Task<(bool Success, int? PlayerId)> ValidateAndReconnectAsync(string sessionToken, string newConnectionId) => await SessionManager.ValidateAndReconnectAsync(sessionToken, newConnectionId).ConfigureAwait(false);
        public async Task<Collection<Session>> GetOnlineSessionsAsync() => await SessionManager.GetOnlineSessionsAsync().ConfigureAwait(false);
        public async Task<Dictionary<string, object>> GetSessionStatsAsync() => await SessionManager.GetSessionStatsAsync().ConfigureAwait(false);

        public ClientConnection? GetConnection(string connectionId)
        {
            _connections.TryGetValue(connectionId, out var connection);
            return connection;
        }

        public IEnumerable<ClientConnection> GetAllConnections()
        {
            return _connections.Values.ToList();
        }

        public async Task<Dictionary<string, object>> GetServerStatsAsync()
        {
            var sessionStats = await SessionManager.GetSessionStatsAsync().ConfigureAwait(false);
            var connectionStats = new Dictionary<string, object>
            {
                ["ConnectedClients"] = ConnectedClients,
                ["ServerUptime"] = DateTime.UtcNow - _serverStartTime,
                ["IsRunning"] = IsRunning
            };

            var combinedStats = new Dictionary<string, object>(sessionStats);
            foreach (var stat in connectionStats)
            {
                combinedStats[stat.Key] = stat.Value;
            }

            return combinedStats;
        }

        private readonly DateTime _serverStartTime = DateTime.UtcNow;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !_isDisposed)
            {
                StopAsync().GetAwaiter().GetResult();
                _cancellationTokenSource?.Dispose();
                _tcpListener?.Stop();
                _tcpListener?.Dispose();
                SessionManager?.Dispose();
            }
            _isDisposed = true;
        }

        private void ThrowIfDisposed()
        {
            if (_isDisposed)
                ObjectDisposedException.ThrowIf(_isDisposed, nameof(GameServer));
        }
    }
}