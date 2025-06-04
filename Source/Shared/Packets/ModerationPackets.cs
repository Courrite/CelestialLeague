using CelestialLeague.Shared.Models;
using CelestialLeague.Shared.Enums;

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

        public ModerationActionPacket(string moderatorId, string targetUserId, ModerationActionType actionType, string? reason = null)
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
        public required string SessionToken { get; set; }
        public ModerationActionType ActionType { get; set; }
        public string? TargetUsername { get; set; }
        public string? ActionId { get; set; } = Guid.NewGuid().ToString();

        public ModerationActionResponsePacket(bool success = true)
        {
            Success = success;
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
        public required string SessionToken { get; set; }
        public required int TargetUserId { get; set; }
        public string? Reason { get; set; }

        public PlayerKickPacket(string sessionToken, int targetUserId, string? reason = null)
        {
            SessionToken = sessionToken;
            TargetUserId = targetUserId;
            Reason = reason;
        }

        public override bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(SessionToken) &&
                TargetUserId > 0;
        }
    }

    public class PlayerBanPacket : BasePacket
    {
        public override PacketType Type => PacketType.PlayerBan;
        public required string SessionToken { get; set; }
        public int TargetUserId;
        public DateTime BanExpiration;
        public string? Reason;
        public string? Evidence;

        public PlayerBanPacket(int targetUserId, DateTime banExpiration, string? reason = null, string? evidence = null)
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
        public required string SessionToken { get; set; }
        public int TargetUserId;

        public PlayerUnbanPacket(int playerid)
        {
            TargetUserId = playerid;
        }

        public override bool IsValid()
        {
            return TargetUserId > 0;
        }
    }

    public class PlayerMutePacket : BasePacket
    {
        public override PacketType Type => PacketType.PlayerMute;
        public required string SessionToken { get; set; }
        public int TargetUserId;
        public DateTime MuteExpiration;
        public string? Reason;
        public string? Evidence;

        public PlayerMutePacket(int targetUserId, DateTime muteExpiration, string? reason = null, string? evidence = null)
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
        public override PacketType Type => PacketType.PlayerMute;
        public required string SessionToken { get; set; }
        public int TargetUserId;

        public PlayerUnmutePacket(int targetUserId, DateTime muteExpiration)
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
        public required string SessionToken { get; set; }
        public int TargetUserId;
        public string? Reason;
        public string? Evidence;
        public required string WarnId = Guid.NewGuid().ToString();

        public PlayerWarnPacket(int targetUserId, string? reason = null, string? evidence = null)
        {
            TargetUserId = targetUserId;
            Reason = reason;
            Evidence = evidence;
        }

        public override bool IsValid()
        {
            return TargetUserId > 0;
        }
    }

    public class PlayerRemoveWarnPacket : BasePacket
    {
        public override PacketType Type => PacketType.PlayerRemoveWarn;
        public required string SessionToken { get; set; }
        public int TargetUserId;
        public string WarnId;

        public PlayerRemoveWarnPacket(int targetUserId, string warnId)
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
        public required string SessionToken { get; set; }
        public required string Message { get; set; }
        public AnnouncementType AnnouncementType { get; set; } = AnnouncementType.General;
        public AnnouncementPriority Priority { get; set; } = AnnouncementPriority.Normal;
        public int DisplayDurationSeconds { get; set; } = 10;

        public ServerAnnouncementPacket(string sessionToken, string message)
        {
            SessionToken = sessionToken;
            Message = message;
        }

        public override bool IsValid()
        {
            return base.IsValid() &&
                   !string.IsNullOrWhiteSpace(SessionToken) &&
                   !string.IsNullOrWhiteSpace(Message) &&
                   Message.Length <= 500 &&
                   Enum.IsDefined(typeof(AnnouncementType), AnnouncementType) &&
                   Enum.IsDefined(typeof(AnnouncementPriority), Priority) &&
                   DisplayDurationSeconds > 0;
        }
    }
}