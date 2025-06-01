public enum MatchResult
{
    // Primary outcomes
    Victory,
    Defeat,
    
    // Special cases
    Draw,              // Both players finish simultaneously
    Forfeit,           // Player quit/surrendered
    Disconnect,        // Player disconnected
    Timeout,           // Match exceeded time limit
    
    // Error states
    Cancelled,         // Match cancelled before completion
    InvalidResult,     // Data corruption or validation failure
}
