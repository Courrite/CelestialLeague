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

    // matchamking (20-39)
    QueueRequest = 20,
    QueueResponse = 21,
    MatchFound = 22,
    QueueCancel = 23,
    QueueCancelResponse = 24,
    MatchAccept = 25,
    MatchAcceptResponse = 26,
    MatchDecline = 27,
    MatchDeclineResponse = 28,

    // gaem (40-69)
    GameStart = 40,
    GameEnd = 41,
    PlayerPosition = 42,
    PlayerDeath = 43,
    PlayerRespawn = 44,
    PlayerFinish = 45,
    GamePause = 46,
    GameResume = 47,
    GameState = 48,
    LevelProgress = 49,

    // chat (70-89)
    ChatMessage = 70,
    ChatResponse = 71,
    PrivateMessage = 72,
    PrivateMessageResponse = 73,

    // system (90-109)
    Heartbeat = 90,
    HeartbeatResponse = 91,
    Disconnect = 92,
    DisconnectResponse = 93,
    Ping = 94,
    PingResponse = 95,
    Error = 96,
    Acknowledgment = 97
}
