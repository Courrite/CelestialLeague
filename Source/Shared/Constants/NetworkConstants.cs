public static class NetworkConstants
{
    // connection settings
    public const int DefaultTcpPort = 7777;
    public const int DefaultUdpPort = 7778;
    public const int VoicePort = 7779;
    public const int MinPort = 1024;
    public const int MaxPort = 65535;

    // connection management
    public const int ConnectionTimeoutMs = 60000; // 1 minute
    public const int HeartbeatIntervalMs = 30000; // 30 seconds
    public const int PingTimeoutMs = 5000; // 5 seconds
    public const int MaxReconnectAttempts = 3;
    public const int MaxConnections = 1000;
    public const int MaxConnectionsPerIp = 5;
    public const int ConnectionBacklog = 100;

    // packet configuration
    public const int MaxPacketSize = 4096; // bytes
    public const int MaxVoicePacketSize = 1024; // bytes - reduced for voice
    public const int PacketHeaderSize = 13; // bytes
    public const int TcpBufferSize = 8192;
    public const int UdpBufferSize = 1024;
    public const int CompressionThreshold = 512; // bytes

    // game update rate limiting
    public const int ServerTickRate = 60; // server runs at 60hz
    public const int MaxPositionUpdatesPerSecond = 30; // client can send max 30 position updates/sec
    public const int MaxGameEventsPerSecond = 20; // max game events per second
    public const int MaxInputsPerSecond = 60; // max input packets per second
    public const int PositionUpdateIntervalMs = 33; // minimum 33ms between position updates (30hz)

    // rate limiting - much stricter now
    public const int MaxMessagesPerMinute = 60;
    public const int MaxMatchmakingRequestsPerMinute = 10;
    public const int MaxFriendRequestsPerHour = 20;
    public const int MaxUdpPacketsPerSecond = 60; // total udp packets per second per client
    public const int MaxTcpPacketsPerSecond = 20; // total tcp packets per second per client
    public const int RateLimitWindowSeconds = 60;
    public const int BurstAllowance = 10; // allow short bursts above the limit
    public const int MaxDeserializationErrors = 10;

    // packet drop protection
    public const int PacketDropThreshold = 100; // drop packets if queue exceeds this
    public const int SuspiciousActivityThreshold = 150; // packets/sec that triggers investigation
    public const int AutoDisconnectThreshold = 200; // packets/sec that triggers auto-disconnect

    // chat network limits
    public const int MaxMessageLength = 500; // characters
    public const int MaxChannelNameLength = 50;

    // matchmaking network
    public const int MatchmakingTimeoutMs = 120000; // 2 minutes
    public const int MaxSearchRange = 300; // mmr difference
    public const int QueueUpdateIntervalMs = 5000; // 5 seconds

    // protocol & security
    public const byte ProtocolVersion = 1;
    public const string MagicHeader = "CLPK"; // celestialleague packet
    public const bool EnableCompression = true;
    public const bool EnableEncryption = false; // for future use
    public const int EncryptionKeySize = 256;

    // network quality
    public const int PingSamples = 5;
    public const int GoodPingThresholdMs = 50;
    public const int FairPingThresholdMs = 100;
    public const int PoorPingThresholdMs = 200;
    public const int PacketLossThresholdPercent = 5;

    // timeouts & intervals
    public const int SocketTimeoutMs = 30000;
    public const int KeepAliveIntervalMs = 60000;
    public const int NetworkStatsUpdateIntervalMs = 1000;
    public const int ConnectionRetryDelayMs = 5000;

    // buffer management
    public const int MinBufferSize = 1024;
    public const int MaxBufferSize = 65536;
    public const int BufferGrowthFactor = 2;

    // network events
    public const int MaxQueuedPackets = 1000;
    public const int PacketQueueWarningThreshold = 750;
    public const int NetworkCongestionThreshold = 80; // percentage

    // client-side rate limiting hints
    public const int RecommendedClientUpdateRate = 30; // hz - what we tell clients to use
    public const int MinUpdateIntervalMs = 16; // 60fps max for any updates
    public const int PositionSmoothingMs = 100; // client-side smoothing window
}