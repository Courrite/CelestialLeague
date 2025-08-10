namespace CelestialLeague.Shared.Constants
{
    public static class Network
    {
        // Ports
        public const int DefaultTcpPort = 7777;
        public const int DefaultUdpPort = 7778;
        public const int MinPort = 1024;
        public const int MaxPort = 65535;

        // Packet sizes
        public const int MaxPacketSize = 4096;
        public const int PacketHeaderSize = 18; // 4 magic + 1 version + 1 type + 4 length + 8 timestamp
        public const string MagicHeader = "CLPK"; // Magic ID for packet validation
        public const byte ProtocolVersion = 1;

        // Ping thresholds (ms)
        public const int PingTimeoutMs = 1000;
        public const int GoodPingThresholdMs = 50;
        public const int FairPingThresholdMs = 100;
        public const int PoorPingThresholdMs = 200;

        // Packet loss threshold (%)
        public const int PacketLossThresholdPercent = 5;

        // Packet flood & limits
        public const int MaxPositionUpdatesPerSecond = 30;
        public const int MaxGameEventsPerSecond = 10;
        public const int MaxMessagesPerMinute = 60;
        public const int MaxUdpPacketsPerSecond = 100;

        // Position updates
        public const int PositionUpdateIntervalMs = 50;

        // Compression
        public const bool EnableCompression = true;
        public const int CompressionThreshold = 512; // bytes

        // Buffer sizes
        public const int MinBufferSize = 256;
        public const int MaxBufferSize = 8192;
        public const int BufferGrowthFactor = 2;

        // Rate limiting
        public const int MaxReconnectAttempts = 5;
        public const int ConnectionRetryDelayMs = 500;

        // Timeouts
        public const int ConnectionTimeoutMs = 60000;
        public const int SocketTimeoutMs = 30000;
        public const int MatchmakingTimeoutMs = 45000;

        // Bandwidth/congestion
        public const double NetworkCongestionThreshold = 85.0; // %

        // Queue management
        public const int PacketQueueWarningThreshold = 500;
        public const int PacketDropThreshold = 1000;

        // Security/flood detection
        public const int SuspiciousActivityThreshold = 200; // PPS
        public const int AutoDisconnectThreshold = 500; // PPS

        // Limits
        public const int MaxDeserializationErrors = 10;
        public const int HeartbeatIntervalMs = 30000;
        public const int MaxConnections = 1000;
        public const int MaxConnectionsPerIp = 5;
        public const int MaxMessageLength = 1024;
    }

    public static class Game
    {
        public const int StartingMMR = 1000;
        public const int MinMMR = 0;
        public const int MaxMMR = 5000;
        public const int PlacementMatches = 10;
        public const int MaxPlayersPerMatch = 2;
        public const int MatchCountdownSeconds = 3;
        public const int MaxMatchDurationMinutes = 10;
        public const int MaxFriends = 200;
        public const int MaxUsernameLength = 20;
        public const int MinUsernameLength = 3;

        // Rating changes
        public const int MinRatingChange = 10;
        public const int MaxRatingChange = 50;
        public const int NewPlayerKFactor = 32;
        public const int EstablishedPlayerKFactor = 16;

        // Replays
        public const int MaxReplayDurationMinutes = 15;
        public const int MaxReplaysPerPlayer = 50;
        public const int ReplayRetentionDays = 30;

        // Seasons
        public static readonly TimeSpan SeasonDuration = TimeSpan.FromDays(90);
        public static readonly DateTime FirstSeasonStart = new DateTime(2099, 12, 12, 12, 12, 12, DateTimeKind.Utc);
        public const float SeasonResetPercentage = 0.8f; // 80% of current MMR
    }

    public static class Security
    {
        public const int SessionTokenLength = 64;
        public const int SessionTimeoutMinutes = 30;
        public const int MaxConcurrentSessions = 3;
        public const int MinPasswordLength = 8;
        public const int MaxPasswordLength = 128;
        public const int PasswordSaltLength = 32;
        public const int PasswordHashLength = 32;
        public const int PasswordHashIterations = 10000;
        public const int MaxLoginAttemptsPerMinute = 5;
        public const int MaxFailedLoginAttempts = 5;
        public const int AccountLockoutMinutes = 15;
        public const int DefaultTrustScore = 100;
        public const int SecureTokenLength = 32;
        public const int MaxMatchmakingRequestsPerMinute = 10;
        public const int MaxReportReasonLength = 500;
    }

    public static class Version
    {
        public const string CurrentClient = "1.0.0";
        public const string CurrentServer = "1.0.0";
        public const string MinSupportedClient = "1.0.0";
        public const string MinSupportedServer = "1.0.0";
        public const string ForceUpdateThreshold = "0.9.0";
        public const string Pattern = @"^\d+\.\d+\.\d+$";
    }
}