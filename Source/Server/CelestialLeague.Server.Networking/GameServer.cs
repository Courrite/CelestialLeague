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
    public sealed class GameServer : IDisposable
    {
        public Logger Logger { get; }
        public IPAddress IPAddress { get; }
        public int Port { get; }
        public SessionManager SessionManager { get; }
        public GameDbContext GameDbContext { get; }
        public AuthenticationService AuthenticationService { get; }
        public PacketProcessor PacketProcessor { get; }

        private TcpListener? _tcpListener;
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        private readonly ConcurrentDictionary<string, ClientConnection> _connections = new();
        private readonly DateTime _serverStartTime = DateTime.UtcNow;
        private Task? _acceptTask;
        private bool _running;
        private bool _disposed;

        public bool IsRunning => _running && !_disposed;
        public int ConnectedClients => _connections.Count;

        public GameServer(IPAddress ipAddress, int port, Logger logger, string? connectionString = null)
        {
            IPAddress = ipAddress;
            Port = port;
            Logger = logger ?? new Logger();
            SessionManager = new SessionManager();
            GameDbContext = new GameDbContextFactory().CreateDbContext([connectionString ?? "Data Source=celestial_league.db"]);
            AuthenticationService = new AuthenticationService(this);
            PacketProcessor = new PacketProcessor(this, HandlerFactory.CreateAllHandlers(this));
        }

        public void StartAsync()
        {
            ObjectDisposedException.ThrowIf(_disposed, nameof(GameServer));
            if (_running) throw new InvalidOperationException("Server already running");

            _tcpListener = new TcpListener(IPAddress, Port);
            _tcpListener.Start();
            _running = true;

            Logger.Info($"Server started on {IPAddress}:{Port}");
            _acceptTask = AcceptClientsAsync(_cancellationTokenSource.Token);
            Logger.Info("Ready to accept connections");
        }

        public async Task StopAsync()
        {
            if (!_running || _disposed) return;

            Logger.Info("Stopping server...");
            _running = false;
            await _cancellationTokenSource.CancelAsync().ConfigureAwait(false);
            _tcpListener?.Stop();

            if (_acceptTask != null)
            {
                try { await _acceptTask.ConfigureAwait(false); }
                catch (OperationCanceledException) { }
            }

            await DisconnectAllClientsAsync().ConfigureAwait(false);
            Logger.Info("Server stopped");
        }

        private async Task AcceptClientsAsync(CancellationToken cancellationToken)
        {
            while (_running && !cancellationToken.IsCancellationRequested)
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
                catch (ObjectDisposedException) when (!_running)
                {
                    break;
                }
                catch (Exception ex)
                {
                    if (_running)
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

                connection.OnDisconnected += (sender, e) =>
                {
                    Logger.Info($"Client disconnected: {connection.ConnectionID}");
                    _ = SessionManager.DisconnectAsync(connection.ConnectionID);
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
                    _ = SessionManager.DisconnectAsync(connection.ConnectionID);
                    connection.Dispose();
                }
                tcpClient?.Close();
            }
        }

        private async Task DisconnectAllClientsAsync()
        {
            var tasks = _connections.Values.Select(c => c.DisconnectAsync()).ToArray();
            if (tasks.Length > 0)
                await Task.WhenAll(tasks).ConfigureAwait(false);
            _connections.Clear();
        }

        // Session management shortcuts
        public Task<string> CreateSessionAsync(int playerId) => SessionManager.CreateSessionAsync(playerId);

        public async Task<Session?> GetSessionAsync(string sessionToken)
        {
            if (!await SessionManager.IsSessionValidAsync(sessionToken).ConfigureAwait(false)) return null;

            var playerId = await SessionManager.GetPlayerIdAsync(sessionToken).ConfigureAwait(false);
            if (playerId == null) return null;

            var sessions = await SessionManager.GetActiveSessionsForPlayerAsync(playerId.Value).ConfigureAwait(false);
            return sessions.FirstOrDefault(s => s.SessionToken == sessionToken);
        }

        public Task<bool> IsSessionValidAsync(string sessionToken) => SessionManager.IsSessionValidAsync(sessionToken);
        public Task InvalidateSessionAsync(string sessionToken) => SessionManager.InvalidateSessionAsync(sessionToken);
        public Task<bool> SetConnectionAsync(string sessionToken, string connectionId) => SessionManager.SetConnectionAsync(sessionToken, connectionId);
        public Task<Session?> GetSessionByConnectionAsync(string connectionId) => SessionManager.GetSessionByConnectionAsync(connectionId);
        public Task<(bool Success, int? PlayerId)> ValidateAndReconnectAsync(string sessionToken, string newConnectionId) => SessionManager.ValidateAndReconnectAsync(sessionToken, newConnectionId);
        public Task<Collection<Session>> GetOnlineSessionsAsync() => SessionManager.GetOnlineSessionsAsync();

        public ClientConnection? GetConnection(string connectionId)
        {
            _connections.TryGetValue(connectionId, out var connection);
            return connection;
        }

        public IEnumerable<ClientConnection> GetAllConnections() => _connections.Values.ToList();

        public async Task<Dictionary<string, object>> GetServerStatsAsync()
        {
            var sessionStats = await SessionManager.GetSessionStatsAsync().ConfigureAwait(false);
            var serverStats = new Dictionary<string, object>
            {
                ["ConnectedClients"] = ConnectedClients,
                ["ServerUptime"] = DateTime.UtcNow - _serverStartTime,
                ["IsRunning"] = IsRunning
            };

            foreach (var stat in sessionStats)
                serverStats[stat.Key] = stat.Value;

            return serverStats;
        }

        public void Dispose()
        {
            if (_disposed) return;

            try
            {
                StopAsync().GetAwaiter().GetResult();
            }
            catch { }

            _cancellationTokenSource?.Dispose();
            _tcpListener?.Dispose();
            SessionManager?.Dispose();
            _disposed = true;
            GC.SuppressFinalize(this);
        }
    }
}