using CelestialLeague.Shared.Enums;

namespace CelestialLeague.Shared.Packets
{
    public class ChatMessagePacket : BasePacket
    {
        public override PacketType Type => PacketType.ChatMessage;
        public required string SessionToken { get; set; }
        public required string Message { get; set; }
        public ChatChannelType ChannelType { get; set; } = ChatChannelType.General;
        public string? MatchId { get; set; }

        public ChatMessagePacket(string sessionToken, string message, ChatChannelType channelType = ChatChannelType.General)
        {
            SessionToken = sessionToken;
            Message = message;
            ChannelType = channelType;
        }

        public override bool IsValid()
        {
            return base.IsValid() &&
                   !string.IsNullOrWhiteSpace(SessionToken) &&
                   !string.IsNullOrWhiteSpace(Message) &&
                   Message.Length <= GameConstants.MaxChatMessageLength &&
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

        public ChatResponsePacket(bool success = true)
        {
            Success = success;
        }

        protected override bool ValidateSuccessResponse()
        {
            return true;
        }
    }

    public class PrivateMessagePacket : BasePacket
    {
        public override PacketType Type => PacketType.PrivateMessage;
        public required string SessionToken { get; set; }
        public required int TargetUserId { get; set; }
        public required string Message { get; set; }

        public PrivateMessagePacket(string sessionToken, int targetUserId, string message)
        {
            SessionToken = sessionToken;
            TargetUserId = targetUserId;
            Message = message;
        }

        public override bool IsValid()
        {
            return base.IsValid() &&
                   !string.IsNullOrWhiteSpace(SessionToken) &&
                   TargetUserId > 0 &&
                   !string.IsNullOrWhiteSpace(Message) &&
                   Message.Length <= GameConstants.MaxPrivateMessageLength;
        }
    }

    public class PrivateMessageBroadcastPacket : BasePacket
    {
        public override PacketType Type => PacketType.PrivateMessageBroadcast;
        public required string MessageId { get; set; }
        public required string SenderUsername { get; set; }
        public required string Message { get; set; }
        public bool IsDelivered { get; set; }

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

        public PrivateMessageResponsePacket(bool success = true)
        {
            Success = success;
        }

        protected override bool ValidateSuccessResponse()
        {
            return true;
        }
    }

    public class ChatJoinChannelPacket : BasePacket
    {
        public override PacketType Type => PacketType.ChatJoinChannel;
        public required string SessionToken { get; set; }
        public ChatChannelType ChannelType { get; set; }
        public string? MatchId { get; set; }

        public ChatJoinChannelPacket(string sessionToken, ChatChannelType channelType, string? matchId = null)
        {
            SessionToken = sessionToken;
            ChannelType = channelType;
            MatchId = matchId;
        }

        public override bool IsValid()
        {
            return base.IsValid() &&
                   !string.IsNullOrWhiteSpace(SessionToken) &&
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
        public required string SessionToken { get; set; }
        public ChatChannelType ChannelType { get; set; }
        public string? MatchId { get; set; }

        public ChatLeaveChannelPacket(string sessionToken, ChatChannelType channelType, string? matchId = null)
        {
            SessionToken = sessionToken;
            ChannelType = channelType;
            MatchId = matchId;
        }

        public override bool IsValid()
        {
            return base.IsValid() &&
                   !string.IsNullOrWhiteSpace(SessionToken) &&
                   Enum.IsDefined(typeof(ChatChannelType), ChannelType) &&
                   ChannelType != ChatChannelType.Private;
        }
    }

    public class ChatChannelListRequestPacket : BasePacket
    {
        public override PacketType Type => PacketType.ChatChannelList;
        public required string SessionToken { get; set; }

        public ChatChannelListRequestPacket(string sessionToken)
        {
            SessionToken = sessionToken;
        }

        public override bool IsValid()
        {
            return base.IsValid() &&
                   !string.IsNullOrWhiteSpace(SessionToken);
        }
    }

    public class ChatChannelListResponsePacket : BaseResponse
    {
        public override PacketType Type => PacketType.ChatChannelListResponse;
        public List<string> AvailableChannels { get; set; } = new();
        public List<string> JoinedChannels { get; set; } = new();

        public ChatChannelListResponsePacket(bool success = true)
        {
            Success = success;
        }

        protected override bool ValidateSuccessResponse()
        {
            return AvailableChannels != null && JoinedChannels != null;
        }
    }

    public class ChatUserListRequestPacket : BasePacket
    {
        public override PacketType Type => PacketType.ChatUserList;
        public required string SessionToken { get; set; }
        public ChatChannelType ChannelType { get; set; }
        public string? MatchId { get; set; }

        public ChatUserListRequestPacket(string sessionToken, ChatChannelType channelType, string? matchId = null)
        {
            SessionToken = sessionToken;
            ChannelType = channelType;
            MatchId = matchId;
        }

        public override bool IsValid()
        {
            return base.IsValid() &&
                   !string.IsNullOrWhiteSpace(SessionToken) &&
                   Enum.IsDefined(typeof(ChatChannelType), ChannelType);
        }
    }

    public class ChatUserListResponsePacket : BaseResponse
    {
        public override PacketType Type => PacketType.ChatUserListResponse;
        public ChatChannelType ChannelType { get; set; }
        public List<string> Usernames { get; set; } = new();
        public int TotalUsers { get; set; }

        public ChatUserListResponsePacket(bool success = true)
        {
            Success = success;
        }

        protected override bool ValidateSuccessResponse()
        {
            return Usernames != null &&
                   TotalUsers >= 0 &&
                   Enum.IsDefined(typeof(ChatChannelType), ChannelType);
        }
    }
}
