using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using CelestialLeague.Server.Models;
using CelestialLeague.Server.Networking;
using CelestialLeague.Server.Utils;

namespace CelestialLeague.Server.Core
{
    public class GameServer : IDisposable
    {
        public Logger Logger { get; private set; }
        public IPAddress IPAddress { get; private set; }
        public int Port { get; private set; }

        private TcpListener? _tcpListener;
        private CancellationTokenSource? _cancellationTokenSource;
        private Task? _acceptTask;

        private readonly ConcurrentDictionary<string, ClientConnection> _connections = new();
        private readonly ConcurrentDictionary<string, Session> _sessions = new();

        private bool _isRunning;
        private bool _isDisposed;

        public bool IsRunning => _isRunning && !_isDisposed;
        public int ConnectedClients => _connections.Count;
        public int ActiveSessions => _sessions.Count;

        public GameServer(IPAddress ipAddress, int port, Logger? logger = null)
        {
            IPAddress = ipAddress;
            Port = port;
            Logger = logger ?? new Logger();
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public async Task StartAsync()
        {
            ThrowIfDisposed();
            if (_isRunning)
                throw new InvalidOperationException("Server is already running");

            try
            {
                _tcpListener = new TcpListener(IPAddress, Port);
                _tcpListener.Start();
                _isRunning = true;

                Logger.Info($"server started on {IPAddress}:{Port}");

                _acceptTask = AcceptClientsAsync(_cancellationTokenSource!.Token);

                Logger.Info("server is ready to accept connections");
            }
            catch (Exception ex)
            {
                Logger.Error($"failed to start server: {ex.Message}");
                throw;
            }
        }

        public async Task StopAsync()
        {
            if (!_isRunning || _isDisposed)
                return;

            try
            {
                Logger.Info("stopping server...");

                _isRunning = false;
                _cancellationTokenSource?.Cancel();

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

                Logger.Info("server stopped");
            }
            catch (Exception ex)
            {
                Logger.Error($"error stopping server: {ex.Message}");
            }
        }

        private async Task AcceptClientsAsync(CancellationToken cancellationToken)
        {
            while (_isRunning && !cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var tcpClient = await _tcpListener!.AcceptTcpClientAsync().ConfigureAwait(false);

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
                        Logger.Error($"error accepting client: {ex.Message}");
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

                Logger.Info($"client connected: {connection.ConnectionID} from {connection.RemoteEndpoint}");

                connection.OnDisconnected += (sender, e) =>
                {
                    Logger.Info($"client disconnected: {connection.ConnectionID}");
                };

                await connection.StartAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Logger.Error($"error handling client: {ex.Message}");
            }
            finally
            {
                if (connection != null)

                    _connections.TryRemove(connection.ConnectionID, out _);

                if (connection.Session != null)
                {
                    _sessions.TryRemove(connection.Session.SessionToken, out _);
                }

                connection.Dispose();
            }

            tcpClient?.Close();
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
            _sessions.Clear();
        }

        public void AddSession(Session session)
        {
            _sessions.TryAdd(session.SessionToken, session);
        }

        public Session? GetSession(string sessionToken)
        {
            _sessions.TryGetValue(sessionToken, out var session);
            return session;
        }

        public void RemoveSession(string sessionToken)
        {
            _sessions.TryRemove(sessionToken, out _);
        }

        public ClientConnection? GetConnection(string connectionId)
        {
            _connections.TryGetValue(connectionId, out var connection);
            return connection;
        }

        public IEnumerable<ClientConnection> GetAllConnections()
        {
            return _connections.Values.ToList();
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;

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
            }
            _isDisposed = true;
        }

        private void ThrowIfDisposed()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(GameServer));
        }
    }
}
