using System.Diagnostics.CodeAnalysis;
using CelestialLeague.Shared.Enums;

namespace CelestialLeague.Shared.Packets
{
    public class ChatMessagePacket : BasePacket
    {
        public override PacketType Type => PacketType.ChatMessage;
        public required string Message { get; set; }
        public ChatChannelType ChannelType { get; set; } = ChatChannelType.General;
        public string? MatchId { get; set; }

        public ChatMessagePacket(string message, ChatChannelType channelType = ChatChannelType.General)
        {
            Message = message;
            ChannelType = channelType;
            CorrelationId = GenerateCorrelationId();
        }

        public override bool IsValid()
        {
            return base.IsValid() &&
                   !string.IsNullOrWhiteSpace(Message) &&
                   Message.Length <= ChatConstants.MaxMessageLength &&
                   Enum.IsDefined(typeof(ChatChannelType), ChannelType) &&
                   ValidateChannelAccess();
        }

        private bool ValidateChannelAccess()
        {
            if (ChannelType == ChatChannelType.Announcements)
                return false;
            if (ChannelType == ChatChannelType.Match ||
                ChannelType == ChatChannelType.Spectator ||
                ChannelType == ChatChannelType.PostMatch)
            {
                return !string.IsNullOrWhiteSpace(MatchId);
            }
            return true;
        }
    }

    public class ChatMessageBroadcastPacket : BasePacket
    {
        public override PacketType Type => PacketType.ChatMessageBroadcast;
        public required string MessageId { get; set; }
        public required string Username { get; set; }
        public required string Message { get; set; }
        public ChatChannelType ChannelType { get; set; }
        public string? MatchId { get; set; }
        public UserRole SenderRole { get; set; } = UserRole.None;

        [SetsRequiredMembers]
        public ChatMessageBroadcastPacket(string messageId, string username, string message, ChatChannelType channelType)
        {
            MessageId = messageId;
            Username = username;
            Message = message;
            ChannelType = channelType;
        }

        public override bool IsValid()
        {
            return base.IsValid() &&
                   !string.IsNullOrWhiteSpace(MessageId) &&
                   !string.IsNullOrWhiteSpace(Username) &&
                   !string.IsNullOrWhiteSpace(Message) &&
                   Username.Length >= GameConstants.MinUsernameLength &&
                   Username.Length <= GameConstants.MaxUsernameLength &&
                   Enum.IsDefined(typeof(ChatChannelType), ChannelType);
        }
    }

    public class ChatResponsePacket : BaseResponse
    {
        public override PacketType Type => PacketType.ChatResponse;

        public ChatResponsePacket(uint? requestCorrelationId = null, bool success = true)
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

    public class PrivateMessagePacket : BasePacket
    {
        public override PacketType Type => PacketType.PrivateMessage;
        public required int TargetUserId { get; set; }
        public required string Message { get; set; }

        public PrivateMessagePacket(int targetUserId, string message)
        {
            TargetUserId = targetUserId;
            Message = message;
            CorrelationId = GenerateCorrelationId();
        }

        public override bool IsValid()
        {
            return base.IsValid() &&
                   TargetUserId > 0 &&
                   !string.IsNullOrWhiteSpace(Message) &&
                   Message.Length <= ChatConstants.MaxPrivateMessageLength;
        }
    }

    public class PrivateMessageBroadcastPacket : BasePacket
    {
        public override PacketType Type => PacketType.PrivateMessageBroadcast;
        public required string MessageId { get; set; }
        public required string SenderUsername { get; set; }
        public required string Message { get; set; }
        public bool IsDelivered { get; set; }

        [SetsRequiredMembers]
        public PrivateMessageBroadcastPacket(string messageId, string senderUsername, string message)
        {
            MessageId = messageId;
            SenderUsername = senderUsername;
            Message = message;
        }

        public override bool IsValid()
        {
            return base.IsValid() &&
                   !string.IsNullOrWhiteSpace(MessageId) &&
                   !string.IsNullOrWhiteSpace(SenderUsername) &&
                   !string.IsNullOrWhiteSpace(Message) &&
                   SenderUsername.Length >= GameConstants.MinUsernameLength &&
                   SenderUsername.Length <= GameConstants.MaxUsernameLength;
        }
    }

    public class PrivateMessageResponsePacket : BaseResponse
    {
        public override PacketType Type => PacketType.PrivateMessageResponse;

        public PrivateMessageResponsePacket(uint? requestCorrelationId = null, bool success = true)
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

    public class ChatJoinChannelPacket : BasePacket
    {
        public override PacketType Type => PacketType.ChatJoinChannel;
        public ChatChannelType ChannelType { get; set; }
        public string? MatchId { get; set; }

        public ChatJoinChannelPacket(ChatChannelType channelType, string? matchId = null)
        {
            ChannelType = channelType;
            MatchId = matchId;
        }

        public override bool IsValid()
        {
            return base.IsValid() &&
                   Enum.IsDefined(typeof(ChatChannelType), ChannelType) &&
                   ValidateChannelJoin();
        }

        private bool ValidateChannelJoin()
        {
            if (ChannelType == ChatChannelType.Match ||
                ChannelType == ChatChannelType.Spectator ||
                ChannelType == ChatChannelType.PostMatch)
            {
                return !string.IsNullOrWhiteSpace(MatchId);
            }
            if (ChannelType == ChatChannelType.Private)
                return false;
            return true;
        }
    }

    public class ChatLeaveChannelPacket : BasePacket
    {
        public override PacketType Type => PacketType.ChatLeaveChannel;
        public ChatChannelType ChannelType { get; set; }
        public string? MatchId { get; set; }

        public ChatLeaveChannelPacket(ChatChannelType channelType, string? matchId = null)
        {
            ChannelType = channelType;
            MatchId = matchId;
        }

        public override bool IsValid()
        {
            return base.IsValid() &&
                   Enum.IsDefined(typeof(ChatChannelType), ChannelType) &&
                   ChannelType != ChatChannelType.Private;
        }
    }

    public class ChatChannelListRequestPacket : BasePacket
    {
        public override PacketType Type => PacketType.ChatChannelList;

        public ChatChannelListRequestPacket()
        {
            CorrelationId = GenerateCorrelationId();
        }

        public override bool IsValid()
        {
            return base.IsValid();
        }
    }

    public class ChatChannelListResponsePacket : BaseResponse
    {
        public override PacketType Type => PacketType.ChatChannelListResponse;
        public List<string> AvailableChannels { get; set; } = new();
        public List<string> JoinedChannels { get; set; } = new();

        public ChatChannelListResponsePacket(uint? requestCorrelationId = null, bool success = true)
        {
            Success = success;
            if (requestCorrelationId.HasValue)
                CorrelationId = requestCorrelationId.Value;
        }

        protected override bool ValidateSuccessResponse()
        {
            return AvailableChannels != null && JoinedChannels != null;
        }
    }

    public class ChatUserListRequestPacket : BasePacket
    {
        public override PacketType Type => PacketType.ChatUserList;
        public ChatChannelType ChannelType { get; set; }
        public string? MatchId { get; set; }

        public ChatUserListRequestPacket(ChatChannelType channelType, string? matchId = null)
        {
            ChannelType = channelType;
            MatchId = matchId;
            CorrelationId = GenerateCorrelationId();
        }

        public override bool IsValid()
        {
            return base.IsValid() &&
                   Enum.IsDefined(typeof(ChatChannelType), ChannelType);
        }
    }

    public class ChatUserListResponsePacket : BaseResponse
    {
        public override PacketType Type => PacketType.ChatUserListResponse;
        public ChatChannelType ChannelType { get; set; }
        public List<string> Users { get; set; } = new();
        public Dictionary<string, UserRole> UserRoles { get; set; } = new();

        public ChatUserListResponsePacket(uint? requestCorrelationId = null, bool success = true)
        {
            Success = success;
            if (requestCorrelationId.HasValue)
                CorrelationId = requestCorrelationId.Value;
        }

        protected override bool ValidateSuccessResponse()
        {
            return Users != null &&
                   UserRoles != null &&
                   Enum.IsDefined(typeof(ChatChannelType), ChannelType);
        }
    }
}
