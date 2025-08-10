using System.Diagnostics.CodeAnalysis;
using CelestialLeague.Shared.Constants;
using CelestialLeague.Shared.Enum;
using CelestialLeague.Shared.Models;
using CelestialLeague.Shared.Utils;

namespace CelestialLeague.Shared.Packets
{
    public class LoginRequestPacket : BasePacket
    {
        public override PacketType Type => PacketType.LoginRequest;
        public required string Username { get; set; } = string.Empty;
        public required string Password { get; set; } = string.Empty;
        public string ClientVersion { get; set; } = Constants.Version.CurrentClient;
        public bool RememberMe { get; set; } = false;

        public LoginRequestPacket() : base()
        {
            Username = string.Empty;
            Password = string.Empty;
        }

        [SetsRequiredMembers]
        public LoginRequestPacket(string username, string password, bool rememberMe = false, string? clientVersion = null) : base(true)
        {
            Username = username;
            Password = password;
            RememberMe = rememberMe;
            ClientVersion = clientVersion ?? Constants.Version.CurrentClient;
        }

        public override bool IsValid()
        {
            return base.IsValid() &&
                   !string.IsNullOrWhiteSpace(Username) &&
                   !string.IsNullOrWhiteSpace(Password) &&
                   Username.Length >= Constants.Game.MinUsernameLength &&
                   Username.Length <= Constants.Game.MaxUsernameLength &&
                   Password.Length >= 6 &&
                   VersionUtils.IsValidVersionFormat(ClientVersion);
        }
    }

    public class LoginResponsePacket : BaseResponse
    {
        public override PacketType Type => PacketType.LoginResponse;
        public PlayerInfo? Player { get; set; }
        public string? SessionToken { get; set; }
        public string ServerVersion { get; set; } = Constants.Version.CurrentServer;
        public int OnlinePlayerCount { get; set; }

        public LoginResponsePacket() : base()
        {
        }

        public LoginResponsePacket(uint? requestCorrelationId = null, bool success = true) : base()
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
        public required string Username { get; set; } = string.Empty;
        public required string Password { get; set; } = string.Empty;
        public string ClientVersion { get; set; } = Constants.Version.CurrentClient;

        public RegisterRequestPacket() : base()
        {
            Username = string.Empty;
            Password = string.Empty;
        }

        [SetsRequiredMembers]
        public RegisterRequestPacket(string username, string password, string? clientVersion = null) : base(true)
        {
            Username = username;
            Password = password;
            ClientVersion = clientVersion ?? Constants.Version.CurrentClient;
        }

        public override bool IsValid()
        {
            return base.IsValid() &&
                   !string.IsNullOrWhiteSpace(Username) &&
                   !string.IsNullOrWhiteSpace(Password) &&
                   Username.Length >= Constants.Game.MinUsernameLength &&
                   Username.Length <= Constants.Game.MaxUsernameLength &&
                   Password.Length >= 6 &&
                   Validation.IsValidUsername(Username) &&
                   VersionUtils.IsValidVersionFormat(ClientVersion);
        }
    }

    public class RegisterResponsePacket : BaseResponse
    {
        public override PacketType Type => PacketType.RegisterResponse;
        public PlayerInfo? Player { get; set; }
        public string? SessionToken { get; set; }
        public string ServerVersion { get; set; } = Constants.Version.CurrentServer;

        public RegisterResponsePacket() : base()
        {
        }

        public RegisterResponsePacket(uint? requestCorrelationId = null, bool success = true) : base()
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

        public LogoutRequestPacket() : base()
        {
        }

        public LogoutRequestPacket(bool generateCorrelationId) : base(generateCorrelationId)
        {
        }

        public override bool IsValid()
        {
            return base.IsValid();
        }
    }

    public class LogoutResponsePacket : BaseResponse
    {
        public override PacketType Type => PacketType.LogoutResponse;

        public LogoutResponsePacket() : base()
        {
        }

        public LogoutResponsePacket(uint? requestCorrelationId = null, bool success = true) : base()
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

        public ValidateSessionRequestPacket() : base()
        {
        }

        public ValidateSessionRequestPacket(bool generateCorrelationId) : base(generateCorrelationId)
        {
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

        public ValidateSessionResponsePacket() : base()
        {
        }

        public ValidateSessionResponsePacket(uint? requestCorrelationId = null, bool success = true) : base()
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
        public required string CurrentPassword { get; set; } = string.Empty;
        public required string NewPassword { get; set; } = string.Empty;

        public ChangePasswordRequestPacket() : base()
        {
            CurrentPassword = string.Empty;
            NewPassword = string.Empty;
        }

        [SetsRequiredMembers]
        public ChangePasswordRequestPacket(string currentPassword, string newPassword) : base(true)
        {
            CurrentPassword = currentPassword;
            NewPassword = newPassword;
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

        public ChangePasswordResponsePacket() : base()
        {
        }

        public ChangePasswordResponsePacket(uint? requestCorrelationId = null, bool success = true) : base()
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