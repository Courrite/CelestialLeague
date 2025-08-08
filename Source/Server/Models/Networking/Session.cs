using System.Net;
using CelestialLeague.Shared.Enums;
using CelestialLeague.Shared.Utils;

namespace CelestialLeague.Server.Models
{
    public class Session : IDisposable
    {
        // identity
        public string SessionToken { get; set; } = string.Empty;
        public int PlayerId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? ConnectionId { get; set; } = string.Empty;

        // timing
        public DateTime CreatedAt { get; set; }
        public DateTime LoginTime { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime LastActivity { get; set; }

        // state
        public PlayerStatus Status { get; set; } = PlayerStatus.Online;
        public GameState GameState { get; set; } = GameState.MainMenu;

        // network
        public IPAddress IpAddress { get; set; } = IPAddress.None;
        public int Ping { get; set; } = -1;

        // disposal
        private bool _disposed;

        // properties
        public bool IsExpired => DateTime.UtcNow > ExpiresAt;
        public TimeSpan SessionDuration => DateTime.UtcNow - LoginTime;
        public bool IsDisposed => _disposed;

        // constructor
        public Session(int playerId, string? connectionId, IPAddress? ipAddress)
        {
            SessionToken = SecurityHelpers.GenerateToken();
            PlayerId = playerId;
            ConnectionId = connectionId ??  string.Empty;
            IpAddress = ipAddress ?? IPAddress.None;
            CreatedAt = DateTime.UtcNow;
            LoginTime = DateTime.UtcNow;
            ExpiresAt = DateTime.UtcNow.AddHours(SecurityConstants.SessionDurationHours);
            LastActivity = DateTime.UtcNow;
            Status = PlayerStatus.Offline;
        }

        // methods
        public void ExtendSession()
        {
            ThrowIfDisposed();
            ExpiresAt = DateTime.UtcNow.AddHours(SecurityConstants.SessionDurationHours);
        }

        public void UpdateConnection(string newConnectionId)
        {
            ThrowIfDisposed();
            ConnectionId = newConnectionId;
            Status = PlayerStatus.Online;
        }

        public void Disconnect()
        {
            if (_disposed) return;
            
            ConnectionId = string.Empty;
            Status = PlayerStatus.Offline;
        }

        // disposal
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    SessionToken = string.Empty;
                    Username = string.Empty;
                    ConnectionId = string.Empty;

                    Status = PlayerStatus.Offline;
                }

                _disposed = true;
            }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(Session), "Session has been disposed");
        }
    }
}