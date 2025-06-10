using System.Collections.Concurrent;
using System.Net;
using CelestialLeague.Server.Models;
using CelestialLeague.Shared.Enums;

namespace CelestialLeague.Server.Services
{
    public class SessionManager : IDisposable
    {
        private readonly ConcurrentDictionary<string, Session> _sessions = new();
        private readonly ConcurrentDictionary<string, string> _connectionToSession = new();
        private readonly Timer _cleanupTimer;
        private bool _disposed = false;

        public SessionManager()
        {
            _cleanupTimer = new Timer(CleanupExpiredSessions, null, 
                TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));
        }

        public async Task<string> CreateSessionAsync(int playerId)
        {
            var session = new Session(playerId, string.Empty, IPAddress.None);
            var sessionToken = session.SessionToken;
            _sessions.TryAdd(sessionToken, session);
            return sessionToken;
        }

        public async Task<int?> GetPlayerIdAsync(string sessionToken)
        {
            if (!_sessions.TryGetValue(sessionToken, out var session))
                return null;

            if (session.ExpiresAt < DateTime.UtcNow)
            {
                _sessions.TryRemove(sessionToken, out _);
                if (session.ConnectionId != null)
                {
                    _connectionToSession.TryRemove(session.ConnectionId, out _);
                }
                return null;
            }

            session.ExpiresAt = DateTime.UtcNow.AddHours(24);
            session.LastActivity = DateTime.UtcNow;
            return session.PlayerId;
        }

        public async Task<bool> IsSessionValidAsync(string sessionToken)
        {
            _sessions.TryGetValue(sessionToken, out var session);
            if (session == null)
                return false;

            if (session.ExpiresAt < DateTime.UtcNow)
                return false;

            return true;
        }

        public async Task InvalidateSessionAsync(string sessionToken)
        {
            if (_sessions.TryRemove(sessionToken, out var session))
            {
                if (session.ConnectionId != null)
                {
                    _connectionToSession.TryRemove(session.ConnectionId, out _);
                }
            }
        }

        public async Task InvalidateAllSessionsForPlayerAsync(int playerId)
        {
            var sessionsToRemove = _sessions
                .Where(kvp => kvp.Value.PlayerId == playerId)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var sessionToken in sessionsToRemove)
            {
                if (_sessions.TryRemove(sessionToken, out var session))
                {
                    if (session.ConnectionId != null)
                    {
                        _connectionToSession.TryRemove(session.ConnectionId, out _);
                    }
                }
            }
        }

        public async Task<List<Session>> GetActiveSessionsForPlayerAsync(int playerId)
        {
            var activeSessions = _sessions.Values
                .Where(s => s.PlayerId == playerId && s.ExpiresAt > DateTime.UtcNow)
                .ToList();
            return activeSessions;
        }

        public async Task<bool> SetConnectionAsync(string sessionToken, string? connectionId)
        {
            if (!_sessions.TryGetValue(sessionToken, out var session))
                return false;

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

            return true;
        }

        public async Task<string?> GetSessionTokenByConnectionAsync(string connectionId)
        {
            _connectionToSession.TryGetValue(connectionId, out var sessionToken);
            return sessionToken;
        }

        public async Task<Session?> GetSessionByConnectionAsync(string connectionId)
        {
            var sessionToken = await GetSessionTokenByConnectionAsync(connectionId);
            if (sessionToken == null)
                return null;

            _sessions.TryGetValue(sessionToken, out var session);
            return session;
        }

        public async Task<(bool Success, int? PlayerId)> ValidateAndReconnectAsync(string sessionToken, string newConnectionId)
        {
            if (!_sessions.TryGetValue(sessionToken, out var session))
                return (false, null);

            if (session.ExpiresAt <= DateTime.UtcNow)
            {
                _sessions.TryRemove(sessionToken, out _);
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
            var sessionToken = await GetSessionTokenByConnectionAsync(connectionId);
            if (sessionToken != null)
            {
                await SetConnectionAsync(sessionToken, null);
            }
        }

        public async Task<List<Session>> GetOnlineSessionsAsync()
        {
            var onlineSessions = _sessions.Values
                .Where(s => s.Status == PlayerStatus.Online && s.ExpiresAt > DateTime.UtcNow)
                .ToList();
            return onlineSessions;
        }

        private void CleanupExpiredSessions(object? state)
        {
            var currentTime = DateTime.UtcNow;
            var expiredSessions = _sessions
                .Where(kvp => kvp.Value.ExpiresAt <= currentTime)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var sessionToken in expiredSessions)
            {
                if (_sessions.TryRemove(sessionToken, out var session))
                {
                    if (session.ConnectionId != null)
                    {
                        _connectionToSession.TryRemove(session.ConnectionId, out _);
                    }
                }
            }
        }

        public async Task<int> GetActiveSessionCount()
        {
            var count = _sessions.Values
                .Count(s => s.ExpiresAt > DateTime.UtcNow);
            return count;
        }

        public async Task<Dictionary<string, object>> GetSessionStatsAsync()
        {
            var currentTime = DateTime.UtcNow;
            var allSessions = _sessions.Values.ToList();
            
            var totalSessions = allSessions.Count;
            var activeSessions = allSessions.Count(s => s.ExpiresAt > currentTime);
            var onlineSessions = allSessions.Count(s => s.Status == PlayerStatus.Online && s.ExpiresAt > currentTime);
            
            var averageDuration = allSessions.Any() 
                ? allSessions.Average(s => (currentTime - s.CreatedAt).TotalMinutes)
                : 0;

            return new Dictionary<string, object>
            {
                ["TotalSessions"] = totalSessions,
                ["ActiveSessions"] = activeSessions,
                ["OnlineSessions"] = onlineSessions,
                ["AverageSessionDurationMinutes"] = Math.Round(averageDuration, 2)
            };
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _cleanupTimer?.Dispose();
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