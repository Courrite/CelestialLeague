using CelestialLeague.Shared.Enums;
using CelestialLeague.Shared.Models;

namespace CelestialLeague.Shared.Packets
{
    // queue
    public class QueueRequestPacket : BasePacket
    {
        public override PacketType Type => PacketType.QueueRequest;
        public required string SessionToken { get; set; }
        public required MatchGamemode MatchGamemode { get; set; }

        public QueueRequestPacket(string sessionToken, MatchGamemode matchType, int? mmr = null)
        {
            SessionToken = sessionToken;
            MatchGamemode = matchType;
        }

        public override bool IsValid()
        {
            return !string.IsNullOrEmpty(SessionToken);
        }
    }

    public class QueueResponsePacket : BaseResponse
    {
        public override PacketType Type => PacketType.QueueResponse;
        public int EstimatedWaitTime { get; set; }
        public int QueuePosition { get; set; }
        public int PlayersInQueue { get; set; }

        public QueueResponsePacket(bool success = true)
        {
            Success = success;
            TimeStamp = DateTime.UtcNow;
        }

        public override bool IsValid()
        {
            if (string.IsNullOrEmpty(CorrelationId))
                return false;

            if (Success)
            {
                return EstimatedWaitTime >= 0 &&
                       QueuePosition >= 0 &&
                       PlayersInQueue >= 0;
            }

            return !string.IsNullOrWhiteSpace(ErrorMessage) &&
                   ErrorCode.HasValue &&
                   Enum.IsDefined(typeof(ResponseErrorCode), ErrorCode.Value);
        }
    }

    // quuee cancel
    public class QueueCancelRequestPacket : BasePacket
    {
        public override PacketType Type => PacketType.QueueCancel;
        public required string SessionToken { get; set; }

        public QueueCancelRequestPacket(string sessionToken)
        {
            SessionToken = sessionToken;
        }

        public override bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(SessionToken);
        }
    }

    public class QueueCancelResponsePacket : BaseResponse
    {
        public override PacketType Type => PacketType.QueueCancelResponse;

        public QueueCancelResponsePacket() : base()
        {
        }

        public override bool IsValid()
        {
            if (string.IsNullOrEmpty(CorrelationId))
                return false;

            if (!Success)
            {
                return !string.IsNullOrWhiteSpace(ErrorMessage) &&
                       ErrorCode.HasValue &&
                   Enum.IsDefined(typeof(ResponseErrorCode), ErrorCode.Value);
            }

            return true;
        }
    }

    // match found notif
    public class MatchFoundPacket : BasePacket
    {
        public override PacketType Type => PacketType.MatchFound;
        public required string MatchId { get; set; } = Guid.NewGuid().ToString();
        public required PlayerInfo Opponent { get; set; }
        public required string LevelName { get; set; }
        public MatchGamemode MatchGamemode { get; set; }
        public int AcceptTimeoutSeconds { get; set; } = 30;

        public MatchFoundPacket(PlayerInfo opponent, string LevelName, MatchGamemode matchType = MatchGamemode.Ranked)
        {
            Opponent = opponent;
            MatchGamemode = matchType;
        }

        public override bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(MatchId) &&
                   Opponent is not null &&
                   !string.IsNullOrWhiteSpace(LevelName) &&
                   Enum.IsDefined(typeof(MatchGamemode), MatchGamemode) &&
                   AcceptTimeoutSeconds > 0;
        }
    }

    // match accept
    public class MatchAcceptRequestPacket : BasePacket
    {
        public override PacketType Type => PacketType.MatchAccept;
        public required string SessionToken { get; set; }
        public required string MatchId { get; set; }

        public MatchAcceptRequestPacket(string sessionToken, string matchId)
        {
            SessionToken = sessionToken;
            MatchId = matchId;
        }

        public override bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(SessionToken) &&
                   !string.IsNullOrWhiteSpace(MatchId);
        }
    }

    public class MatchAcceptResponsePacket : BaseResponse
    {
        public override PacketType Type => PacketType.MatchAcceptResponse;
        public bool WaitingForOpponent { get; set; }

        public MatchAcceptResponsePacket() : base()
        {
        }

        protected override bool ValidateSuccessResponse()
        {
            return WaitingForOpponent;
        }
    }

    // match decline
    public class MatchDeclineRequestPacket : BasePacket
    {
        public override PacketType Type => PacketType.MatchDecline;
        public required string SessionToken { get; set; }
        public required string MatchId { get; set; }

        public MatchDeclineRequestPacket(string sessionToken, string matchId)
        {
            SessionToken = sessionToken;
            MatchId = matchId;
        }

        public override bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(SessionToken) &&
                   !string.IsNullOrWhiteSpace(MatchId);
        }
    }

    public class MatchDeclineResponsePacket : BaseResponse
    {
        public override PacketType Type => PacketType.MatchDeclineResponse;
        public bool ReturnToQueue { get; set; } = true;

        public MatchDeclineResponsePacket(bool success = true)
        {
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
                       ErrorCode.HasValue &&
                   Enum.IsDefined(typeof(ResponseErrorCode), ErrorCode.Value);
            }

            return true;
        }
    }

    public class MatchmakingStatusPacket : BasePacket
    {
        public override PacketType Type => PacketType.MatchmakingStatus;
        public MatchmakingStatus Status { get; set; }
        public int QueuePosition { get; set; }
        public int EstimatedWaitTimeSeconds { get; set; }
        public int PlayersInQueue { get; set; }
        public int AverageWaitTimeSeconds { get; set; }
        public string? StatusMessage { get; set; }
        public Dictionary<string, object> AdditionalData { get; set; } = new();

        public MatchmakingStatusPacket(MatchmakingStatus status)
        {
            Status = status;
        }

        public override bool IsValid()
        {
            return Enum.IsDefined(typeof(MatchmakingStatus), Status) &&
                   QueuePosition >= 0 &&
                   EstimatedWaitTimeSeconds >= 0 &&
                   PlayersInQueue >= 0;
        }
    }
}