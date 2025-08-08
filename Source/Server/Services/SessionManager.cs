using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Net;
using CelestialLeague.Server.Models;
using CelestialLeague.Shared.Enums;

namespace CelestialLeague.Server.Services
{
    public class SessionManager : IAsyncDisposable, IDisposable
    {
        private readonly ConcurrentDictionary<string, Session> _sessions = new();
        private readonly ConcurrentDictionary<string, string> _connectionToSession = new();
        private readonly Timer _cleanupTimer;
        private readonly SemaphoreSlim _cleanupSemaphore = new(1, 1);
        private bool _disposed;

        public SessionManager()
        {
            _cleanupTimer = new Timer(async _ => await CleanupExpiredSessionsAsync().ConfigureAwait(false),
                null, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));
        }

        public Task<string> CreateSessionAsync(int playerId)
        {
            var session = new Session(playerId, string.Empty, IPAddress.None);
            var sessionToken = session.SessionToken;
            _sessions.TryAdd(sessionToken, session);
            return Task.FromResult(sessionToken);
        }

        public async Task<int?> GetPlayerIdAsync(string sessionToken)
        {
            if (!_sessions.TryGetValue(sessionToken, out var session))
                return null;

            if (session.ExpiresAt < DateTime.UtcNow)
            {
                await RemoveExpiredSessionAsync(sessionToken, session).ConfigureAwait(false);
                return null;
            }

            session.ExpiresAt = DateTime.UtcNow.AddHours(24);
            session.LastActivity = DateTime.UtcNow;
            return session.PlayerId;
        }

        public async Task<bool> IsSessionValidAsync(string sessionToken)
        {
            if (!_sessions.TryGetValue(sessionToken, out var session))
                return false;

            if (session.ExpiresAt < DateTime.UtcNow)
            {
                await RemoveExpiredSessionAsync(sessionToken, session).ConfigureAwait(false);
                return false;
            }

            return true;
        }

        public async Task InvalidateSessionAsync(string sessionToken)
        {
            Session? sessionToDispose = null;
            try
            {
                if (_sessions.TryRemove(sessionToken, out sessionToDispose))
                {
                    if (sessionToDispose.ConnectionId != null)
                    {
                        _connectionToSession.TryRemove(sessionToDispose.ConnectionId, out _);
                    }

                    if (sessionToDispose is IAsyncDisposable asyncDisposable)
                    {
                        await asyncDisposable.DisposeAsync().ConfigureAwait(false);
                        sessionToDispose = null;
                    }
                    else if (sessionToDispose is IDisposable disposable)
                    {
                        disposable.Dispose();
                        sessionToDispose = null;
                    }
                }
            }
            finally
            {
                sessionToDispose?.Dispose();
            }
        }

        public async Task InvalidateAllSessionsForPlayerAsync(int playerId)
        {
            var sessionsToRemove = _sessions
                .Where(kvp => kvp.Value.PlayerId == playerId)
                .Select(kvp => kvp.Key)
                .ToList();

            var removalTasks = sessionsToRemove.Select(async sessionToken =>
            {
                Session? sessionToDispose = null;
                try
                {
                    if (_sessions.TryRemove(sessionToken, out sessionToDispose))
                    {
                        if (sessionToDispose.ConnectionId != null)
                        {
                            _connectionToSession.TryRemove(sessionToDispose.ConnectionId, out _);
                        }

                        if (sessionToDispose is IAsyncDisposable asyncDisposable)
                        {
                            await asyncDisposable.DisposeAsync().ConfigureAwait(false);
                            sessionToDispose = null;
                        }
                        else if (sessionToDispose is IDisposable disposable)
                        {
                            disposable.Dispose();
                            sessionToDispose = null;
                        }
                    }
                }
                finally
                {
                    sessionToDispose?.Dispose();
                }
            });

            await Task.WhenAll(removalTasks).ConfigureAwait(false);
        }

        public Task<Collection<Session>> GetActiveSessionsForPlayerAsync(int playerId)
        {
            var activeSessions = new Collection<Session>(_sessions.Values
                .Where(s => s.PlayerId == playerId && s.ExpiresAt > DateTime.UtcNow)
                .ToList());
            return Task.FromResult(activeSessions);
        }

        public Task<bool> SetConnectionAsync(string sessionToken, string? connectionId)
        {
            if (!_sessions.TryGetValue(sessionToken, out var session))
                return Task.FromResult(false);

            if (session.ConnectionId != null)
            {
                _connectionToSession.TryRemove(session.ConnectionId, out _);
            }

            session.ConnectionId = connectionId;
            session.Status = connectionId != null ? PlayerStatus.Online : PlayerStatus.Offline;
            session.LastActivity = DateTime.UtcNow;

            if (connectionId != null)
            {
                _connectionToSession.TryAdd(connectionId, sessionToken);
            }

            return Task.FromResult(true);
        }

        public Task<string?> GetSessionTokenByConnectionAsync(string connectionId)
        {
            _connectionToSession.TryGetValue(connectionId, out var sessionToken);
            return Task.FromResult(sessionToken);
        }

        public Task<Session?> GetSessionByConnectionAsync(string connectionId)
        {
            var sessionToken = _connectionToSession.TryGetValue(connectionId, out var token) ? token : null;
            if (sessionToken == null)
                return Task.FromResult<Session?>(null);

            _sessions.TryGetValue(sessionToken, out var session);
            return Task.FromResult(session);
        }

        public async Task<(bool Success, int? PlayerId)> ValidateAndReconnectAsync(string sessionToken, string newConnectionId)
        {
            if (!_sessions.TryGetValue(sessionToken, out var session))
                return (false, null);

            if (session.ExpiresAt <= DateTime.UtcNow)
            {
                await RemoveExpiredSessionAsync(sessionToken, session).ConfigureAwait(false);
                return (false, null);
            }

            if (session.ConnectionId != null)
            {
                _connectionToSession.TryRemove(session.ConnectionId, out _);
            }

            session.ConnectionId = newConnectionId;
            session.Status = PlayerStatus.Online;
            session.LastActivity = DateTime.UtcNow;
            session.ExpiresAt = DateTime.UtcNow.AddHours(24);

            _connectionToSession.TryAdd(newConnectionId, sessionToken);

            return (true, session.PlayerId);
        }

        public async Task DisconnectAsync(string connectionId)
        {
            var sessionToken = _connectionToSession.TryGetValue(connectionId, out var token) ? token : null;
            if (sessionToken != null)
            {
                await SetConnectionAsync(sessionToken, null).ConfigureAwait(false);
            }
        }

        public Task<Collection<Session>> GetOnlineSessionsAsync()
        {
            var onlineSessions = new Collection<Session>(_sessions.Values
                .Where(s => s.Status == PlayerStatus.Online && s.ExpiresAt > DateTime.UtcNow)
                .ToList());
            return Task.FromResult(onlineSessions);
        }

        public Task<int> GetActiveSessionCountAsync()
        {
            var count = _sessions.Values
                .Count(s => s.ExpiresAt > DateTime.UtcNow);
            return Task.FromResult(count);
        }

        public Task<Dictionary<string, object>> GetSessionStatsAsync()
        {
            var currentTime = DateTime.UtcNow;
            var allSessions = _sessions.Values.ToList();

            var totalSessions = allSessions.Count;
            var activeSessions = allSessions.Count(s => s.ExpiresAt > currentTime);
            var onlineSessions = allSessions.Count(s => s.Status == PlayerStatus.Online && s.ExpiresAt > currentTime);

            var averageDuration = allSessions.Count != 0
                ? allSessions.Average(s => (currentTime - s.CreatedAt).TotalMinutes)
                : 0;

            var stats = new Dictionary<string, object>
            {
                ["TotalSessions"] = totalSessions,
                ["ActiveSessions"] = activeSessions,
                ["OnlineSessions"] = onlineSessions,
                ["AverageSessionDurationMinutes"] = Math.Round(averageDuration, 2)
            };

            return Task.FromResult(stats);
        }

        private async Task CleanupExpiredSessionsAsync()
        {
            if (!await _cleanupSemaphore.WaitAsync(100).ConfigureAwait(false))
                return;

            try
            {
                var currentTime = DateTime.UtcNow;
                var expiredSessions = _sessions
                    .Where(kvp => kvp.Value.ExpiresAt <= currentTime)
                    .Select(kvp => new { Token = kvp.Key, Session = kvp.Value })
                    .ToList();

                if (expiredSessions.Count == 0)
                    return;

                var cleanupTasks = expiredSessions.Select(async expired =>
                {
                    Session? sessionToDispose = null;
                    try
                    {
                        if (_sessions.TryRemove(expired.Token, out sessionToDispose))
                        {
                            if (sessionToDispose.ConnectionId != null)
                            {
                                _connectionToSession.TryRemove(sessionToDispose.ConnectionId, out _);
                            }

                            if (sessionToDispose is IAsyncDisposable asyncDisposable)
                            {
                                await asyncDisposable.DisposeAsync().ConfigureAwait(false);
                                sessionToDispose = null;
                            }
                            else if (sessionToDispose is IDisposable disposable)
                            {
                                disposable.Dispose();
                                sessionToDispose = null;
                            }
                        }
                    }
                    finally
                    {
                        sessionToDispose?.Dispose();
                    }
                });

                await Task.WhenAll(cleanupTasks).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SessionManager] Error during cleanup: {ex.Message}");
            }
            finally
            {
                _cleanupSemaphore.Release();
            }
        }

        private async Task RemoveExpiredSessionAsync(string sessionToken, Session session)
        {
            Session? sessionToDispose = null;
            try
            {
                if (_sessions.TryRemove(sessionToken, out sessionToDispose))
                {
                    if (sessionToDispose.ConnectionId != null)
                    {
                        _connectionToSession.TryRemove(sessionToDispose.ConnectionId, out _);
                    }

                    if (sessionToDispose is IAsyncDisposable asyncDisposable)
                    {
                        await asyncDisposable.DisposeAsync().ConfigureAwait(false);
                        sessionToDispose = null;
                    }
                    else if (sessionToDispose is IDisposable disposable)
                    {
                        disposable.Dispose();
                        sessionToDispose = null;
                    }
                }
            }
            finally
            {
                sessionToDispose?.Dispose();
            }
        }

        public Task<bool> ExtendSessionAsync(string sessionToken, TimeSpan extension)
        {
            if (!_sessions.TryGetValue(sessionToken, out var session))
                return Task.FromResult(false);

            if (session.ExpiresAt < DateTime.UtcNow)
                return Task.FromResult(false);

            session.ExpiresAt = DateTime.UtcNow.Add(extension);
            session.LastActivity = DateTime.UtcNow;
            return Task.FromResult(true);
        }

        public async Task<Session?> GetSessionAsync(string sessionToken)
        {
            if (!_sessions.TryGetValue(sessionToken, out var session))
                return null;

            if (session.ExpiresAt < DateTime.UtcNow)
            {
                await RemoveExpiredSessionAsync(sessionToken, session).ConfigureAwait(false);
                return null;
            }

            return session;
        }

        public Task UpdateSessionActivityAsync(string sessionToken)
        {
            if (_sessions.TryGetValue(sessionToken, out var session))
            {
                session.LastActivity = DateTime.UtcNow;
            }
            return Task.CompletedTask;
        }

        public Task<List<Session>> GetSessionsByPlayerAsync(int playerId)
        {
            var sessions = _sessions.Values
                .Where(s => s.PlayerId == playerId && s.ExpiresAt > DateTime.UtcNow)
                .ToList();
            return Task.FromResult(sessions);
        }

        public Task<bool> IsPlayerOnlineAsync(int playerId)
        {
            var isOnline = _sessions.Values.Any(s =>
                s.PlayerId == playerId &&
                s.Status == PlayerStatus.Online &&
                s.ExpiresAt > DateTime.UtcNow);
            return Task.FromResult(isOnline);
        }

        public async ValueTask DisposeAsync()
        {
            if (!_disposed)
            {
                await _cleanupTimer.DisposeAsync().ConfigureAwait(false);

                await _cleanupSemaphore.WaitAsync().ConfigureAwait(false);
                try
                {
                    var disposeTasks = _sessions.Values.Select(async session =>
                    {
                        if (session is IAsyncDisposable asyncDisposable)
                        {
                            await asyncDisposable.DisposeAsync().ConfigureAwait(false);
                        }
                        else if (session is IDisposable disposable)
                        {
                            disposable.Dispose();
                        }
                    });

                    await Task.WhenAll(disposeTasks).ConfigureAwait(false);
                }
                finally
                {
                    _cleanupSemaphore.Release();
                    _cleanupSemaphore.Dispose();
                }

                _sessions.Clear();
                _connectionToSession.Clear();
                _disposed = true;
            }

            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _cleanupTimer.Dispose();

                    _cleanupSemaphore.Wait();
                    try
                    {
                        var disposeTasks = _sessions.Values.Select(async session =>
                        {
                            if (session is IAsyncDisposable asyncDisposable)
                            {
                                await asyncDisposable.DisposeAsync().ConfigureAwait(false);
                            }
                            else if (session is IDisposable disposable)
                            {
                                disposable.Dispose();
                            }
                        });

                        Task.WhenAll(disposeTasks).GetAwaiter().GetResult();
                    }
                    finally
                    {
                        _cleanupSemaphore.Release();
                        _cleanupSemaphore.Dispose();
                    }

                    _sessions.Clear();
                    _connectionToSession.Clear();
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}