public enum MatchOutcome
{
    InProgress,     // Match is still running
    Completed,      // Normal finish with winner/loser
    Draw,           // Tie game
    Abandoned,      // Someone left/disconnected/forfeited
    Cancelled       // Match cancelled before starting
}
