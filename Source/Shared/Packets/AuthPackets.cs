using CelestialLeague.Shared.Enums;
using CelestialLeague.Shared.Models;
using CelestialLeague.Shared.Utils;

namespace CelestialLeague.Shared.Packets
{
    public class LoginRequestPacket : BasePacket
    {
        public override PacketType Type => PacketType.LoginRequest;
        public DateTime TimeStamp { get; set; } = DateTime.UtcNow;
        public required string Username { get; set; }
        public required string Password { get; set; }
        public string ClientVersion { get; set; } = VersionConstants.CURRENT_CLIENT_VERSION;
        public bool RememberMe { get; set; } = false;

        public LoginRequestPacket(string username, string password, bool rememberMe = false, string? clientVersion = null)
        {
            Username = username;
            Password = password;
            RememberMe = rememberMe;
            ClientVersion = clientVersion ?? VersionConstants.CURRENT_CLIENT_VERSION;
            TimeStamp = DateTime.UtcNow;
            CorrelationId = GenerateCorrelationId();
        }

        public override bool IsValid()
        {
            return base.IsValid() &&
                   !string.IsNullOrWhiteSpace(Username) &&
                   !string.IsNullOrWhiteSpace(Password) &&
                   Username.Length >= GameConstants.MinUsernameLength &&
                   Username.Length <= GameConstants.MaxUsernameLength &&
                   Password.Length >= 6 &&
                   VersionUtils.IsValidVersionFormat(ClientVersion);
        }
    }

    public class LoginResponsePacket : BaseResponse
    {
        public override PacketType Type => PacketType.LoginResponse;
        public PlayerInfo? Player { get; set; }
        public string ServerVersion { get; set; } = VersionConstants.CURRENT_SERVER_VERSION;
        public string? MessageOfTheDay { get; set; }
        public int OnlinePlayerCount { get; set; }

        public LoginResponsePacket(uint? requestCorrelationId = null, bool success = true)
        {
            Success = success;
            if (requestCorrelationId.HasValue)
                CorrelationId = requestCorrelationId.Value;
        }

        protected override bool ValidateSuccessResponse()
        {
            return Player is not null &&
                   OnlinePlayerCount >= 0 &&
                   VersionUtils.IsValidVersionFormat(ServerVersion);
        }
    }

    public class RegisterRequestPacket : BasePacket
    {
        public override PacketType Type => PacketType.RegisterRequest;
        public DateTime TimeStamp { get; set; } = DateTime.UtcNow;
        public required string Username { get; set; }
        public required string Password { get; set; }
        public string? Email { get; set; }
        public string ClientVersion { get; set; } = VersionConstants.CURRENT_CLIENT_VERSION;

        public RegisterRequestPacket(string username, string password, string? email = null, string? clientVersion = null)
        {
            Username = username;
            Password = password;
            Email = email;
            ClientVersion = clientVersion ?? VersionConstants.CURRENT_CLIENT_VERSION;
            TimeStamp = DateTime.UtcNow;
            CorrelationId = GenerateCorrelationId();
        }

        public override bool IsValid()
        {
            return base.IsValid() &&
                   !string.IsNullOrWhiteSpace(Username) &&
                   !string.IsNullOrWhiteSpace(Password) &&
                   Username.Length >= GameConstants.MinUsernameLength &&
                   Username.Length <= GameConstants.MaxUsernameLength &&
                   Password.Length >= 6 &&
                   Validation.IsValidUsername(Username) &&
                   (string.IsNullOrEmpty(Email) || Validation.IsValidEmail(Email)) &&
                   VersionUtils.IsValidVersionFormat(ClientVersion);
        }
    }

    public class RegisterResponsePacket : BaseResponse
    {
        public override PacketType Type => PacketType.RegisterResponse;
        public PlayerInfo? Player { get; set; }
        public string ServerVersion { get; set; } = VersionConstants.CURRENT_SERVER_VERSION;
        public string? WelcomeMessage { get; set; }

        public RegisterResponsePacket(uint? requestCorrelationId = null, bool success = true)
        {
            Success = success;
            if (requestCorrelationId.HasValue)
                CorrelationId = requestCorrelationId.Value;
        }

        protected override bool ValidateSuccessResponse()
        {
            return Player is not null &&
                   VersionUtils.IsValidVersionFormat(ServerVersion);
        }
    }

    public class LogoutRequestPacket : BasePacket
    {
        public override PacketType Type => PacketType.LogoutRequest;
        public DateTime TimeStamp { get; set; } = DateTime.UtcNow;

        public LogoutRequestPacket()
        {
            TimeStamp = DateTime.UtcNow;
            CorrelationId = GenerateCorrelationId();
        }

        public override bool IsValid()
        {
            return base.IsValid();
        }
    }

    public class LogoutResponsePacket : BaseResponse
    {
        public override PacketType Type => PacketType.LogoutResponse;

        public LogoutResponsePacket(uint? requestCorrelationId = null, bool success = true)
        {
            Success = success;
            if (requestCorrelationId.HasValue)
                CorrelationId = requestCorrelationId.Value;
        }

        protected override bool ValidateSuccessResponse()
        {
            return true;
        }
    }

    public class ValidateSessionRequestPacket : BasePacket
    {
        public override PacketType Type => PacketType.ValidateSessionRequest;
        public DateTime TimeStamp { get; set; } = DateTime.UtcNow;

        public ValidateSessionRequestPacket()
        {
            TimeStamp = DateTime.UtcNow;
            CorrelationId = GenerateCorrelationId();
        }

        public override bool IsValid()
        {
            return base.IsValid();
        }
    }

    public class ValidateSessionResponsePacket : BaseResponse
    {
        public override PacketType Type => PacketType.ValidateSessionResponse;
        public PlayerInfo? Player { get; set; }
        public DateTime? ExpiresAt { get; set; }

        public ValidateSessionResponsePacket(uint? requestCorrelationId = null, bool success = true)
        {
            Success = success;
            if (requestCorrelationId.HasValue)
                CorrelationId = requestCorrelationId.Value;
        }

        protected override bool ValidateSuccessResponse()
        {
            return Player is not null && ExpiresAt.HasValue;
        }
    }

    public class ChangePasswordRequestPacket : BasePacket
    {
        public override PacketType Type => PacketType.ChangePasswordRequest;
        public DateTime TimeStamp { get; set; } = DateTime.UtcNow;
        public required string CurrentPassword { get; set; }
        public required string NewPassword { get; set; }

        public ChangePasswordRequestPacket(string currentPassword, string newPassword)
        {
            CurrentPassword = currentPassword;
            NewPassword = newPassword;
            TimeStamp = DateTime.UtcNow;
            CorrelationId = GenerateCorrelationId();
        }

        public override bool IsValid()
        {
            return base.IsValid() &&
                   !string.IsNullOrWhiteSpace(CurrentPassword) &&
                   !string.IsNullOrWhiteSpace(NewPassword) &&
                   NewPassword.Length >= 6 &&
                   CurrentPassword != NewPassword;
        }
    }

    public class ChangePasswordResponsePacket : BaseResponse
    {
        public override PacketType Type => PacketType.ChangePasswordResponse;

        public ChangePasswordResponsePacket(uint? requestCorrelationId = null, bool success = true)
        {
            Success = success;
            if (requestCorrelationId.HasValue)
                CorrelationId = requestCorrelationId.Value;
        }

        protected override bool ValidateSuccessResponse()
        {
            return true;
        }
    }
}