using CelestialLeague.Shared.Models;
using CelestialLeague.Shared.Enums;
using System.Diagnostics.CodeAnalysis;

namespace CelestialLeague.Shared.Packets
{
    public class ModerationActionPacket : BasePacket
    {
        public override PacketType Type => PacketType.ModerationAction;
        public required int ModeratorId { get; set; }
        public required int TargetUserId { get; set; }
        public ModerationActionType ActionType { get; set; }
        public int? DurationMinutes { get; set; }
        public string? Reason { get; set; }
        public string? Evidence { get; set; }

        public ModerationActionPacket() : base()
        {
        }

        [SetsRequiredMembers]
        public ModerationActionPacket(string moderatorId, string targetUserId, ModerationActionType actionType, string? reason = null) : base(true)
        {
            ModeratorId = int.Parse(moderatorId);
            TargetUserId = int.Parse(targetUserId);
            ActionType = actionType;
            Reason = reason;
        }

        public override bool IsValid()
        {
            return base.IsValid() &&
                ModeratorId > 0 &&
                TargetUserId > 0 &&
                Enum.IsDefined(typeof(ModerationActionType), ActionType);
        }
    }

    public class ModerationActionResponsePacket : BaseResponse
    {
        public override PacketType Type => PacketType.ModerationActionResponse;
        public ModerationActionType ActionType { get; set; }
        public string? TargetUsername { get; set; }
        public string? ActionId { get; set; } = Guid.NewGuid().ToString();

        public ModerationActionResponsePacket() : base()
        {
        }

        public ModerationActionResponsePacket(uint? requestCorrelationId = null, bool success = true) : base()
        {
            Success = success;
            if (requestCorrelationId.HasValue)
                CorrelationId = requestCorrelationId.Value;
        }

        protected override bool ValidateSuccessResponse()
        {
            return Enum.IsDefined(typeof(ModerationActionType), ActionType) &&
                !string.IsNullOrWhiteSpace(TargetUsername) &&
                !string.IsNullOrWhiteSpace(ActionId);
        }
    }

    public class PlayerKickPacket : BasePacket
    {
        public override PacketType Type => PacketType.PlayerKick;
        public required int TargetUserId { get; set; }
        public string? Reason { get; set; }

        public PlayerKickPacket() : base()
        {
        }

        [SetsRequiredMembers]
        public PlayerKickPacket(int targetUserId, string? reason = null) : base()
        {
            TargetUserId = targetUserId;
            Reason = reason;
        }

        public override bool IsValid()
        {
            return TargetUserId > 0;
        }
    }

    public class PlayerBanPacket : BasePacket
    {
        public override PacketType Type => PacketType.PlayerBan;
        public int TargetUserId;
        public DateTime BanExpiration;
        public string? Reason;
        public string? Evidence;

        public PlayerBanPacket() : base()
        {
        }

        public PlayerBanPacket(int targetUserId, DateTime banExpiration, string? reason = null, string? evidence = null) : base()
        {
            TargetUserId = targetUserId;
            BanExpiration = banExpiration;
            Reason = reason;
            Evidence = evidence;
        }

        public override bool IsValid()
        {
            return TargetUserId > 0 &&
                BanExpiration > DateTime.UtcNow;
        }
    }

    public class PlayerUnbanPacket : BasePacket
    {
        public override PacketType Type => PacketType.PlayerUnban;
        public int TargetUserId;

        public PlayerUnbanPacket() : base()
        {
        }

        public PlayerUnbanPacket(int playerId) : base()
        {
            TargetUserId = playerId;
        }

        public override bool IsValid()
        {
            return TargetUserId > 0;
        }
    }

    public class PlayerMutePacket : BasePacket
    {
        public override PacketType Type => PacketType.PlayerMute;
        public int TargetUserId;
        public DateTime MuteExpiration;
        public string? Reason;
        public string? Evidence;

        public PlayerMutePacket() : base()
        {
        }

        public PlayerMutePacket(int targetUserId, DateTime muteExpiration, string? reason = null, string? evidence = null) : base()
        {
            TargetUserId = targetUserId;
            MuteExpiration = muteExpiration;
            Reason = reason;
            Evidence = evidence;
        }

        public override bool IsValid()
        {
            return TargetUserId > 0 &&
                MuteExpiration > DateTime.UtcNow;
        }
    }

    public class PlayerUnmutePacket : BasePacket
    {
        public override PacketType Type => PacketType.PlayerUnmute;
        public int TargetUserId;

        public PlayerUnmutePacket() : base()
        {
        }

        public PlayerUnmutePacket(int targetUserId) : base()
        {
            TargetUserId = targetUserId;
        }

        public override bool IsValid()
        {
            return TargetUserId > 0;
        }
    }

    public class PlayerWarnPacket : BasePacket
    {
        public override PacketType Type => PacketType.PlayerWarn;
        public int TargetUserId;
        public string? Reason;
        public string? Evidence;
        public required string WarnId { get; set; } = Guid.NewGuid().ToString();

        public PlayerWarnPacket() : base()
        {
            WarnId = Guid.NewGuid().ToString();
        }

        [SetsRequiredMembers]
        public PlayerWarnPacket(int targetUserId, string? reason = null, string? evidence = null) : base()
        {
            TargetUserId = targetUserId;
            Reason = reason;
            Evidence = evidence;
            WarnId = Guid.NewGuid().ToString();
        }

        public override bool IsValid()
        {
            return TargetUserId > 0;
        }
    }

    public class PlayerRemoveWarnPacket : BasePacket
    {
        public override PacketType Type => PacketType.PlayerRemoveWarn;
        public int TargetUserId;
        public string WarnId;

        public PlayerRemoveWarnPacket() : base()
        {
            WarnId = string.Empty;
        }

        public PlayerRemoveWarnPacket(int targetUserId, string warnId) : base()
        {
            TargetUserId = targetUserId;
            WarnId = warnId;
        }

        public override bool IsValid()
        {
            return TargetUserId > 0;
        }
    }

    public class ServerAnnouncementPacket : BasePacket
    {
        public override PacketType Type => PacketType.ServerAnnouncement;
        public required string Message { get; set; } = string.Empty;
        public AnnouncementType AnnouncementType { get; set; } = AnnouncementType.General;
        public AnnouncementPriority Priority { get; set; } = AnnouncementPriority.Normal;
        public int DisplayDurationSeconds { get; set; } = 10;

        public ServerAnnouncementPacket() : base()
        {
            Message = string.Empty;
        }

        [SetsRequiredMembers]
        public ServerAnnouncementPacket(string message) : base()
        {
            Message = message;
        }

        public override bool IsValid()
        {
            return base.IsValid() &&
                   !string.IsNullOrWhiteSpace(Message) &&
                   Message.Length <= 500 &&
                   Enum.IsDefined(typeof(AnnouncementType), AnnouncementType) &&
                   Enum.IsDefined(typeof(AnnouncementPriority), Priority) &&
                   DisplayDurationSeconds > 0;
        }
    }
}
