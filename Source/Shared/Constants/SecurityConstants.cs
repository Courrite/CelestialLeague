public static class SecurityConstants
{
    // session management
    public const int SessionTokenLength = 64;
    public const int SessionTimeoutMinutes = 30;
    public const int MaxConcurrentSessions = 3;
    
    // password requirements
    public const int MinPasswordLength = 8;
    public const int MaxPasswordLength = 128;
    public const int PasswordSaltLength = 32;
    
    // rate limiting
    public const int MaxLoginAttemptsPerMinute = 5;
    public const int MaxChatMessagesPerMinute = 30;
    public const int MaxMatchmakingRequestsPerMinute = 10;
    public const int MaxFriendRequestsPerHour = 20;
    
    // token security
    public const int RefreshTokenExpirationDays = 30;
    public const int SecureTokenLength = 32;
    
    // CLAW
    public const int MaxInputsPerSecond = 60;
    public const int SuspiciousInputThreshold = 100;
    public const double MaxAllowedTimeDrift = 0.1; // seconds
    
    // encryption
    public const int EncryptionKeyLength = 256; // bits
    public const int IvLength = 16; // bytes
    
    // account security
    public const int MaxFailedLoginAttempts = 5;
    public const int AccountLockoutMinutes = 15;
    public const int EmailVerificationTokenLength = 6;
    
    // trust system
    public const int DefaultTrustScore = 100;
    public const int MinTrustScore = 0;
    public const int MaxTrustScore = 1000;
    public const int TrustScoreDecayPerReport = 10;
    
    // data validation
    public const int MaxReportReasonLength = 500;
}
