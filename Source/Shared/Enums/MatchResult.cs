namespace CelestialLeague.Shared.Enums
{
    public enum MatchResult
    {
        InProgress = 0, // match still running
        Victory = 1, // player won
        Defeat = 2, // player lost  
        Draw = 3, // tie game
        Forfeit = 4, // player surrendered
        Disconnect = 5, // player disconnected
        Cancelled = 6 // match got cancelled
    }
}