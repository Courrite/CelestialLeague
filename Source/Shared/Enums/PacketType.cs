public enum PacketType
{
    // auth
    LoginRequest = 1,
    LoginResponse = 2,
    RegisterRequest = 3,
    RegisterResponse = 4,

    // matchmaking
    QueueRequest = 10,
    QueueResponse = 11,
    MatchFound = 12,

    // game
    GameStart = 20,
    GameEnd = 21,
    PlayerPosition = 22,

    // chat
    ChatMessage = 30,

    // system
    Heartbeat = 100,
    Disconnect = 101
}