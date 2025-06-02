public enum MatchOutcome
{
    InProgress, // match is still running
    Completed, // normal finish with winner/loser
    Draw, // tie game
    Abandoned, // someone left/disconnected/forfeited
    Cancelled // match cancelled before starting
}
