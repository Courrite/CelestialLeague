﻿public static class GameConstants
{
    // ranking system
    public const int StartingMMR = 1000;
    public const int MinMMR = 0;
    public const int MaxMMR = 5000;
    public const int PlacementMatches = 10;

    // rating changes
    public const int MinRatingChange = 10;
    public const int MaxRatingChange = 50;
    public const int NewPlayerKFactor = 32;
    public const int EstablishedPlayerKFactor = 16;

    // match settings
    public const int MaxPlayersPerMatch = 2;
    public const int MatchCountdownSeconds = 3;
    public const int MaxMatchDurationMinutes = 10;

    // social
    public const int MaxFriends = 200;
    public const int MaxUsernameLength = 20;
    public const int MinUsernameLength = 3;

    // replays
    public const int MaxReplayDurationMinutes = 15;
    public const int MaxReplaysPerPlayer = 50;
    public const int ReplayRetentionDays = 30;

    // seasons
    public static readonly TimeSpan SeasonDuration = TimeSpan.FromDays(90);
    public static readonly DateTime FirstSeasonStart = new DateTime(2099, 12, 12, 12, 12, 12, DateTimeKind.Utc);
    public const float SeasonResetPercentage = 0.8f; // 80% of current MMR
}
