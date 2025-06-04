using CelestialLeague.Shared.Enums;

namespace CelestialLeague.Shared.Packets
{
    public class HeartbeatPacket : BasePacket
    {
        public override PacketType Type => PacketType.Heartbeat;
        public long ClientTimestamp { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        public override bool IsValid()
        {
            return base.IsValid() && ClientTimestamp > 0;
        }
    }

    public class HeartbeatResponsePacket : BasePacket
    {
        public override PacketType Type => PacketType.HeartbeatResponse;
        public long ServerTimestamp { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        public long ClientTimestamp { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        public HeartbeatResponsePacket(long clientTimestamp)
        {
            ClientTimestamp = clientTimestamp;
        }

        public override bool IsValid()
        {
            return base.IsValid() && ClientTimestamp > 0;
        }
    }

    public class DisconnectPacket : BasePacket
    {
        public override PacketType Type => PacketType.Disconnect;
        public required string SessionToken { get; set; }
        public string? Reason { get; set; }

        public DisconnectPacket(string sessionToken, string? reason = null)
        {
            SessionToken = sessionToken;
            Reason = reason;
        }

        public override bool IsValid()
        {
            return base.IsValid() && !string.IsNullOrWhiteSpace(SessionToken);
        }
    }

    public class DisconnectResponsePacket : BaseResponse
    {
        public override PacketType Type => PacketType.DisconnectResponse;

        public DisconnectResponsePacket(bool success = true)
        {
            Success = success;
        }

        protected override bool ValidateSuccessResponse()
        {
            return true;
        }
    }

    public class PingPacket : BasePacket
    {
        public override PacketType Type => PacketType.Ping;
        public new long Timestamp { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        public PingPacket()
        { }

        public override bool IsValid()
        {
            return base.IsValid() && Timestamp > 0;
        }
    }

    public class PingResponsePacket : BasePacket
    {
        public override PacketType Type => PacketType.PingResponse;
        public long OriginalTimestamp { get; set; }
        public long ResponseTimestamp { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        public PingResponsePacket(long originalTimestamp)
        {
            OriginalTimestamp = originalTimestamp;
        }

        public override bool IsValid()
        {
            return base.IsValid() && OriginalTimestamp > 0 && ResponseTimestamp > 0;
        }
    }

    public class ErrorPacket : BasePacket
    {
        public override PacketType Type => PacketType.Error;
        public required ResponseErrorCode ErrorCode { get; set; }
        public required string ErrorMessage { get; set; }
        public string? Details { get; set; }
        public Dictionary<string, object> ErrorData { get; set; } = new();

        public ErrorPacket(ResponseErrorCode errorCode, string errorMessage, string? details = null)
        {
            ErrorCode = errorCode;
            ErrorMessage = errorMessage;
            Details = details;
        }

        public override bool IsValid()
        {
            return base.IsValid() && Enum.IsDefined(typeof(ResponseErrorCode), ErrorCode) && !string.IsNullOrWhiteSpace(ErrorMessage);
        }
    }

    public class AcknowledgmentPacket : BasePacket
    {
        public override PacketType Type => PacketType.Acknowledgment;
        public required string SessionToken { get; set; }
        public required string AcknowledgmentToken { get; set; }

        public AcknowledgmentPacket(string sessionToken, string acknowledgmentToken)
        {
            SessionToken = sessionToken;
            AcknowledgmentToken = acknowledgmentToken;
        }

        public override bool IsValid()
        {
            return base.IsValid() && !string.IsNullOrWhiteSpace(SessionToken) && !string.IsNullOrWhiteSpace(AcknowledgmentToken);
        }
    }

    public class ServerStatusPacket : BasePacket
    {
        public override PacketType Type => PacketType.ServerStatus;
        public int OnlinePlayerCount { get; set; }
        public int ActiveMatches { get; set; }
        public int QueuedPlayers { get; set; }
        public string ServerVersion { get; set; } = VersionConstants.CURRENT_SERVER_VERSION;
        public Dictionary<string, object> StatusData { get; set; } = new();

        public ServerStatusPacket()
        { }

        public override bool IsValid()
        {
            return base.IsValid() && OnlinePlayerCount >= 0 &&
            ActiveMatches >= 0 &&
            QueuedPlayers >= 0;
        }
    }

    public class ServerShutdownPacket : BasePacket
    {
        public override PacketType Type => PacketType.ServerShutdown;
        public int ShutdownInSeconds { get; set; }
        public string? Reason { get; set; }
        public bool Maintenance { get; set; } = false;

        public ServerShutdownPacket(int shutdownInSeconds, string? reason = null)
        {
            ShutdownInSeconds = shutdownInSeconds;
            Reason = reason;
        }

        public override bool IsValid()
        {
            return base.IsValid() && ShutdownInSeconds >= 0;
        }
    }

    public class ForceDisconnectPacket : BasePacket
    {
        public override PacketType Type => PacketType.ForceDisconnect;
        public required string Reason { get; set; }

        public ForceDisconnectPacket(string reason)
        {
            Reason = reason;
        }

        public override bool IsValid()
        {
            return base.IsValid() && !string.IsNullOrWhiteSpace(Reason);
        }
    }

    public class RateLimitWarningPacket : BasePacket
    {
        public override PacketType Type => PacketType.RateLimitWarning;
        public int RequestsPerSecond { get; set; }
        public int MaxAllowed { get; set; }
        public int CooldownSeconds { get; set; }

        public RateLimitWarningPacket(int requestsPerSecond, int maxAllowed, int cooldownSeconds)
        {
            RequestsPerSecond = requestsPerSecond;
            MaxAllowed = maxAllowed;
            CooldownSeconds = cooldownSeconds;
        }

        public override bool IsValid()
        {
            return base.IsValid() && RequestsPerSecond > 0 && MaxAllowed > 0 && CooldownSeconds > 0;
        }
    }
}