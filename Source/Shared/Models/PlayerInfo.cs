namespace CelestialLeague.Shared.Models
{
    public class PlayerInfo
    {
        // credential and profile
        public required string Name { get; set; }
        public required string Password { get; set; }
        public required DateTime CreatedAt { get; set; }
        public required DateTime LastSeen { get; set; }

        // rating
        public int MMR { get; set; } = 0;
        public int Rank { get; set; } = 0;

        // stats
        public int TotalMatches { get; set; } = 0;
        public int Wins { get; set; } = 0;
        public int Losses { get; set; } = 0;
        public float WinRate => TotalMatches > 0 ? (float)Wins / TotalMatches : 0;
        public TimeSpan BestTime { get; set; } = TimeSpan.Zero;
        public int WinStreak { get; set; } = 0;
        public int BestWinStreak { get; set; } = 0;

        // social
        public PlayerStatus PlayerStatus { get; set; } = PlayerStatus.Offline;
    }
}