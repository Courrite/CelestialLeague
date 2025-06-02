using CelestialLeague.Shared.Constants;
using CelestialLeague.Shared.Models;
using CelestialLeague.Shared.Utils;

namespace CelestialLeague.Shared.Packets
{
    // login
    public class LoginRequestPacket : BasePacket
    {
        public override PacketType Type => PacketType.LoginRequest;
        public DateTime TimeStamp { get; set; } = DateTime.UtcNow;
        public required string Username { get; set; }
        public required string Password { get; set; }
        public string ClientVersion { get; set; } = VersionConstants.CURRENT_CLIENT_VERSION;
        public bool RememberMe { get; set; } = false;
        public new string? CorrelationId { get; set; }

        public LoginRequestPacket(string username, string password, bool rememberMe = false, string? clientVersion = null)
        {
            Username = username;
            Password = password;
            RememberMe = rememberMe;
            ClientVersion = clientVersion ?? VersionConstants.CURRENT_CLIENT_VERSION;
            CorrelationId = Guid.NewGuid().ToString();
            TimeStamp = DateTime.UtcNow;
        }

        public override bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(Username) &&
                   !string.IsNullOrWhiteSpace(Password) &&
                   Username.Length >= 3 &&
                   Username.Length <= 20 &&
                   Password.Length >= 6 &&
                   VersionUtils.IsValidVersionFormat(ClientVersion);
        }
    }

    public class LoginResponsePacket : BasePacket
    {
        public override PacketType Type => PacketType.LoginResponse;
        public DateTime TimeStamp { get; set; } = DateTime.UtcNow;
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public string? ErrorCode { get; set; }
        public PlayerInfo? Player { get; set; }
        public string? SessionToken { get; set; }
        public string ServerVersion { get; set; } = VersionConstants.CURRENT_SERVER_VERSION;
        public string? MessageOfTheDay { get; set; }
        public int OnlinePlayerCount { get; set; }
        public new string? CorrelationId { get; set; }

        public LoginResponsePacket(string? correlationId = null, bool success = true)
        {
            CorrelationId = correlationId ?? Guid.NewGuid().ToString();
            Success = success;
            TimeStamp = DateTime.UtcNow;
        }

        public override bool IsValid()
        {
            if (string.IsNullOrEmpty(CorrelationId))
                return false;

            if (!VersionUtils.IsValidVersionFormat(ServerVersion))
                return false;

            if (Success)
            {
                return Player is not null &&
                       !string.IsNullOrWhiteSpace(SessionToken) &&
                       OnlinePlayerCount >= 0;
            }

            return !string.IsNullOrWhiteSpace(ErrorMessage) &&
                   !string.IsNullOrWhiteSpace(ErrorCode);
        }
    }

    // register
    public class RegisterRequestPacket : BasePacket
    {
        public override PacketType Type => PacketType.RegisterRequest;
        public DateTime TimeStamp { get; set; } = DateTime.UtcNow;
        public required string Username { get; set; }
        public required string Password { get; set; }
        public string? Email { get; set; }
        public string ClientVersion { get; set; } = VersionConstants.CURRENT_CLIENT_VERSION;
        public new string? CorrelationId { get; set; }

        public RegisterRequestPacket(string username, string password, string? clientVersion = null)
        {
            Username = username;
            Password = password;
            ClientVersion = clientVersion ?? VersionConstants.CURRENT_CLIENT_VERSION;
            CorrelationId = Guid.NewGuid().ToString();
            TimeStamp = DateTime.UtcNow;
        }

        public override bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(Username) &&
                   !string.IsNullOrWhiteSpace(Password) &&
                   Username.Length >= 3 &&
                   Username.Length <= 20 &&
                   Password.Length >= 6;
        }
    }

    public class RegisterResponsePacket : BasePacket
    {
        public override PacketType Type => PacketType.RegisterResponse;
        public DateTime TimeStamp { get; set; } = DateTime.UtcNow;
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public string? ErrorCode { get; set; }
        public PlayerInfo? Player { get; set; }
        public string? SessionToken { get; set; }
        public string ServerVersion { get; set; } = VersionConstants.CURRENT_SERVER_VERSION;
        public string? WelcomeMessage { get; set; }
        public new string? CorrelationId { get; set; }

        public RegisterResponsePacket(string? correlationId = null, bool success = true)
        {
            CorrelationId = correlationId ?? Guid.NewGuid().ToString();
            Success = success;
            TimeStamp = DateTime.UtcNow;
        }

        public override bool IsValid()
        {
            if (string.IsNullOrEmpty(CorrelationId))
                return false;

            if (!VersionUtils.IsValidVersionFormat(ServerVersion))
                return false;

            if (Success)
            {
                return Player is not null &&
                       !string.IsNullOrWhiteSpace(SessionToken);
            }

            return !string.IsNullOrWhiteSpace(ErrorMessage) &&
                   !string.IsNullOrWhiteSpace(ErrorCode);
        }
    }

    // logout
    public class LogoutRequestPacket : BasePacket
    {
        public override PacketType Type => PacketType.LogoutRequest;
        public DateTime TimeStamp { get; set; } = DateTime.UtcNow;
        public required string SessionToken { get; set; }
        public new string? CorrelationId { get; set; }

        public LogoutRequestPacket(string sessionToken)
        {
            SessionToken = sessionToken;
            CorrelationId = Guid.NewGuid().ToString();
            TimeStamp = DateTime.UtcNow;
        }

        public override bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(SessionToken);
        }
    }

    public class LogoutResponsePacket : BasePacket
    {
        public override PacketType Type => PacketType.LogoutResponse;
        public DateTime TimeStamp { get; set; } = DateTime.UtcNow;
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public string? ErrorCode { get; set; }
        public new string? CorrelationId { get; set; }

        public LogoutResponsePacket(string? correlationId = null, bool success = true)
        {
            CorrelationId = correlationId ?? Guid.NewGuid().ToString();
            Success = success;
            TimeStamp = DateTime.UtcNow;
        }

        public override bool IsValid()
        {
            if (string.IsNullOrEmpty(CorrelationId))
                return false;

            if (!Success)
            {
                return !string.IsNullOrWhiteSpace(ErrorMessage) &&
                       !string.IsNullOrWhiteSpace(ErrorCode);
            }

            return true;
        }
    }

    // session validation
    public class ValidateSessionRequestPacket : BasePacket
    {
        public override PacketType Type => PacketType.ValidateSessionRequest;
        public DateTime TimeStamp { get; set; } = DateTime.UtcNow;
        public required string SessionToken { get; set; }
        public new string? CorrelationId { get; set; }

        public ValidateSessionRequestPacket(string sessionToken)
        {
            SessionToken = sessionToken;
            CorrelationId = Guid.NewGuid().ToString();
            TimeStamp = DateTime.UtcNow;
        }

        public override bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(SessionToken);
        }
    }

    public class ValidateSessionResponsePacket : BasePacket
    {
        public override PacketType Type => PacketType.ValidateSessionResponse;
        public DateTime TimeStamp { get; set; } = DateTime.UtcNow;
        public bool IsValidSession { get; set; }
        public PlayerInfo? Player { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public string? ErrorMessage { get; set; }
        public string? ErrorCode { get; set; }
        public new string? CorrelationId { get; set; }

        public ValidateSessionResponsePacket(string? correlationId = null, bool isValid = true)
        {
            CorrelationId = correlationId ?? Guid.NewGuid().ToString();
            IsValidSession = isValid;
            TimeStamp = DateTime.UtcNow;
        }

        public override bool IsValid()
        {
            if (string.IsNullOrEmpty(CorrelationId))
                return false;

            if (IsValidSession)
            {
                return Player is not null && ExpiresAt.HasValue;
            }

            return !string.IsNullOrWhiteSpace(ErrorMessage) &&
                   !string.IsNullOrWhiteSpace(ErrorCode);
        }
    }

    // password change 
    public class ChangePasswordRequestPacket : BasePacket
    {
        public override PacketType Type => PacketType.ChangePasswordRequest;
        public DateTime TimeStamp { get; set; } = DateTime.UtcNow;
        public required string SessionToken { get; set; }
        public required string CurrentPassword { get; set; }
        public required string NewPassword { get; set; }
        public new string? CorrelationId { get; set; }

        public ChangePasswordRequestPacket(string sessionToken, string currentPassword, string newPassword)
        {
            SessionToken = sessionToken;
            CurrentPassword = currentPassword;
            NewPassword = newPassword;
            CorrelationId = Guid.NewGuid().ToString();
            TimeStamp = DateTime.UtcNow;
        }

        public override bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(SessionToken) &&
                   !string.IsNullOrWhiteSpace(CurrentPassword) &&
                   !string.IsNullOrWhiteSpace(NewPassword) &&
                   NewPassword.Length >= 6 &&
                   CurrentPassword != NewPassword;
        }
    }

    public class ChangePasswordResponsePacket : BasePacket
    {
        public override PacketType Type => PacketType.ChangePasswordResponse;
        public DateTime TimeStamp { get; set; } = DateTime.UtcNow;
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public string? ErrorCode { get; set; }
        public new string? CorrelationId { get; set; }

        public ChangePasswordResponsePacket(string? correlationId = null, bool success = true)
        {
            CorrelationId = correlationId ?? Guid.NewGuid().ToString();
            Success = success;
            TimeStamp = DateTime.UtcNow;
        }

        public override bool IsValid()
        {
            if (string.IsNullOrEmpty(CorrelationId))
                return false;

            if (!Success)
            {
                return !string.IsNullOrWhiteSpace(ErrorMessage) &&
                       !string.IsNullOrWhiteSpace(ErrorCode);
            }

            return true;
        }
    }
}