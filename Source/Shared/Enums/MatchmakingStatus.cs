namespace CelestialLeague.Shared.Enums
{
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