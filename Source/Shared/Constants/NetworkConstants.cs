public static class NetworkConstants
{
    // connection
    public const int DefaultPort = 7777;
    public const int MinPort = 1024;
    public const int MaxPort = 65535;
    public const int ConnectionTimeout = 10000; // ms
    public const int PingTimeout = 5000; // ms
    
    // packets
    public const int MaxPacketSize = 1024; // bytes
    public const int PacketHeaderSize = 13; // bytes
    public const int CompressionThreshold = 512; // bytes
    
    // rate limiting
    public const int MaxMessagesPerMinute = 60;
    public const int MaxMatchmakingRequestsPerMinute = 10;
    public const int MaxFriendRequestsPerHour = 20;
    
    // chat
    public const int MaxMessageLength = 500; // characters
    public const int MaxChannelNameLength = 50;
    
    // matchmaking
    public const int MatchmakingTimeout = 120000; // ms (2 minutes)
    public const int MaxSearchRange = 300; // MMR difference
    
    // voice
    public const int VoicePort = 7778;
    public const int MaxVoicePacketSize = 4096;
}
