public enum MatchResult
{
    // Player-specific results
    Victory,        // Player won
    Defeat,         // Player lost
    Draw,           // Tie

    // Abandonment results
    Forfeit,        // Player surrendered
    Disconnect,     // Player disconnected
    Timeout,        // Player timed out

    // System results
    Cancelled       // Match was cancelled
}