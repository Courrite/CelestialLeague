namespace CelestialLeague.Shared.Enum
{
    public enum MatchGamemode
    {
        Ranked = 1,
    }

    public enum MatchState
    {
        WaitingForPlayers = 0,
        Starting = 1,
        InProgress = 2,
        Paused = 3,
        Ended = 4
    }

    public enum MatchResult
    {
        Victory = 1, // player won
        Defeat = 2, // player lost  
        Draw = 3, // tie game
        Forfeit = 4, // player surrendered
        Disconnect = 5, // player disconnected
        Cancelled = 6 // match got cancelled
    }

    public enum MatchmakingStatus
    {
        NotQueued = 0,
        Searching = 1,
        MatchFound = 2,
        WaitingForAcceptance = 3,
        WaitingForOpponent = 4,
        MatchStarting = 5,
        MatchCancelled = 6,
        QueueTimeout = 7,
        Error = 8
    }
}