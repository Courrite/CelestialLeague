namespace CelestialLeague.Shared.Enums
{
    public enum PacketType
    {
        // auth (1-19)
        LoginRequest = 1,
        LoginResponse = 2,
        RegisterRequest = 3,
        RegisterResponse = 4,
        LogoutRequest = 5,
        LogoutResponse = 6,
        ValidateSessionRequest = 7,
        ValidateSessionResponse = 8,
        ChangePasswordRequest = 9,
        ChangePasswordResponse = 10,
        SessionExpiredNotification = 11,
        SessionRenewRequest = 12,
        SessionRenewResponse = 13,
        // reserved: 14-19 for future auth features

        // matchmaking (20-39)
        QueueRequest = 20,
        QueueResponse = 21,
        MatchFound = 22,
        QueueCancel = 23,
        QueueCancelResponse = 24,
        MatchAccept = 25,
        MatchAcceptResponse = 26,
        MatchDecline = 27,
        MatchDeclineResponse = 28,
        MatchmakingStatus = 29, // queue position, estimated time
                                // reserved: 32-39 for future matchmaking features

        // game (40-69)
        PlayerPosition = 40,
        GameState = 41,
        // PlayerInput = 42, redundant (superseded by PlayerPosition)
        GameEvent = 43,
        // RoomSync = 44, redundant (superseded by PlayerPosition)
        MatchResult = 45,
        JoinGameRequest = 46, // match join
        JoinGameResponse = 47,
        GamePause = 48,
        // GameResume = 49, merged with GamePause
        MatchStateChange = 50, // match officially starts
                               // MatchEnd = 51, merged with MatchStateChange
        PlayerStateChange = 52, // player ready state
                                // PlayerNotReady = 53, merged with PlayerStateChange
        LevelLoad = 54, // level loading sync
                        // reserved: 55-69 for future game features

        // chat (70-89)
        ChatMessage = 70,
        ChatResponse = 71,
        ChatMessageBroadcast = 72,
        PrivateMessage = 73,
        PrivateMessageResponse = 74,
        PrivateMessageBroadcast = 75,
        ChatJoinChannel = 76,
        ChatLeaveChannel = 77,
        ChatChannelList = 78,
        ChatChannelListResponse = 79,
        ChatUserList = 80,
        ChatUserListResponse = 81,
        // reserved: 82-89 for future chat features


        // system (90-109)
        Heartbeat = 90,
        HeartbeatResponse = 91,
        Disconnect = 92,
        DisconnectResponse = 93,
        Ping = 94,
        PingResponse = 95,
        Error = 96,
        Acknowledgment = 97,
        ServerStatus = 98,
        ServerShutdown = 99,
        ForceDisconnect = 100,
        RateLimitWarning = 101,
        // reserved: 102-109 for future system features

        // admin/moderation (110-129) - move from social range
        ModerationAction = 110,
        ModerationActionResponse = 111,
        PlayerKick = 112,
        PlayerBan = 113,
        PlayerUnban = 114,
        PlayerMute = 115,
        PlayerUnmute = 116,
        PlayerWarn = 117,
        PlayerRemoveWarn = 118,
        ServerAnnouncement = 119,
        // reserved: 115-129

        // social: 130-149
        // statistics: 150-169  
        // tournaments: 160-179
        // replays: 180-199
    }
}