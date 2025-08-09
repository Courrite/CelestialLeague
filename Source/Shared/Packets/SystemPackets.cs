using System.Diagnostics.CodeAnalysis;
using CelestialLeague.Shared.Enums;

namespace CelestialLeague.Shared.Packets
{
    public class HeartbeatPacket : BasePacket
    {
        public override PacketType Type => PacketType.Heartbeat;

        public HeartbeatPacket() : base()
        {
        }

        public override bool IsValid()
        {
            return base.IsValid();
        }
    }

    public class DisconnectPacket : BasePacket
    {
        public override PacketType Type => PacketType.Disconnect;
        public string? Reason { get; set; }

        public DisconnectPacket() : base()
        {
        }

        public DisconnectPacket(string? reason = null) : base(true)
        {
            Reason = reason;
        }

        public override bool IsValid()
        {
            return base.IsValid();
        }
    }

    public class DisconnectResponsePacket : BaseResponse
    {
        public override PacketType Type => PacketType.DisconnectResponse;

        public DisconnectResponsePacket() : base()
        {
        }

        public DisconnectResponsePacket(uint? requestCorrelationId = null, bool success = true) : base()
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

    public class PingPacket : BasePacket
    {
        public override PacketType Type => PacketType.Ping;
        public new long Timestamp { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        public PingPacket() : base()
        {
        }

        public PingPacket(bool generateCorrelationId) : base(generateCorrelationId)
        {
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }

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

        public PingResponsePacket() : base()
        {
        }

        public PingResponsePacket(uint? requestCorrelationId, long originalTimestamp) : base()
        {
            OriginalTimestamp = originalTimestamp;
            CorrelationId = requestCorrelationId;
            ResponseTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
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
        public required string ErrorMessage { get; set; } = string.Empty;
        public string? Details { get; set; }
        public Dictionary<string, object> ErrorData { get; set; } = new();

        public ErrorPacket() : base()
        {
            ErrorMessage = string.Empty;
        }

        [SetsRequiredMembers]
        public ErrorPacket(ResponseErrorCode errorCode, string errorMessage, string? details = null) : base()
        {
            ErrorCode = errorCode;
            ErrorMessage = errorMessage;
            Details = details;
        }

        public override bool IsValid()
        {
            return base.IsValid() &&
                   Enum.IsDefined(typeof(ResponseErrorCode), ErrorCode) &&
                   !string.IsNullOrWhiteSpace(ErrorMessage);
        }
    }

    public class AcknowledgmentPacket : BasePacket
    {
        public override PacketType Type => PacketType.Acknowledgment;

        public AcknowledgmentPacket() : base()
        {
        }

        public AcknowledgmentPacket(uint? requestCorrelationId) : base()
        {
            CorrelationId = requestCorrelationId; // Use same ID as original request
        }

        public override bool IsValid()
        {
            return base.IsValid();
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

        public ServerStatusPacket() : base()
        {
        }

        public override bool IsValid()
        {
            return base.IsValid() &&
                   OnlinePlayerCount >= 0 &&
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

        public ServerShutdownPacket() : base()
        {
        }

        public ServerShutdownPacket(int shutdownInSeconds, string? reason = null) : base()
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
        public required string Reason { get; set; } = string.Empty;

        public ForceDisconnectPacket() : base()
        {
            Reason = string.Empty;
        }

        [SetsRequiredMembers]
        public ForceDisconnectPacket(string reason) : base()
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

        public RateLimitWarningPacket() : base()
        {
        }

        public RateLimitWarningPacket(int requestsPerSecond, int maxAllowed, int cooldownSeconds) : base()
        {
            RequestsPerSecond = requestsPerSecond;
            MaxAllowed = maxAllowed;
            CooldownSeconds = cooldownSeconds;
        }

        public override bool IsValid()
        {
            return base.IsValid() &&
                   RequestsPerSecond > 0 &&
                   MaxAllowed > 0 &&
                   CooldownSeconds > 0;
        }
    }
}
