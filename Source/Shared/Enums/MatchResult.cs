public enum MatchResult
{
    // player-specific results
    Victory,        // player won
    Defeat,         // player lost
    Draw,           // tie

    // abandonment results
    Forfeit,        // player surrendered
    Disconnect,     // player disconnected
    Timeout,        // player timed out

    // system results
    Cancelled       // match was cancelled
}