namespace CelestialLeague.Shared.Models
{
    public class PlayerStats
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public int PlayerId { get; set; }

        public int Season { get; set; }

        // match statistics
        public int TotalMatches { get; set; } = 0;
        public int Wins { get; set; } = 0;
        public int Losses { get; set; } = 0;
        public int Draws { get; set; } = 0;

        // abandonment stats
        public int Forfeits { get; set; } = 0;
        public int Disconnects { get; set; } = 0;

        // opponent abandonment
        public int WinsByForfeit { get; set; } = 0;
        public int WinsByDisconnect { get; set; } = 0;

        // rating statistics
        public int CurrentMMR { get; set; } = 1000;
        public int StartingMMR { get; set; } = 1000;
        public int PeakMMR { get; set; } = 1000;
        public int LowestMMR { get; set; } = 1000;
        public int CurrentRank { get; set; } = 0;
        public int PeakRank { get; set; } = 0;

        // game statistics
        public TimeSpan TotalPlayTime { get; set; } = TimeSpan.Zero;
        public double AverageDeaths { get; set; } = 0;
        public int TotalDeaths { get; set; } = 0;
        public int PerfectRuns { get; set; } = 0;

        // streak statistics
        public int CurrentWinStreak { get; set; } = 0;
        public int BestWinStreak { get; set; } = 0;
        public int CurrentLossStreak { get; set; } = 0;
        public int WorstLossStreak { get; set; } = 0;

        // timestamps
        public DateTime? FirstMatchAt { get; set; }
        public DateTime? LastMatchAt { get; set; }

        // computed properties
        public double WinRate => TotalMatches > 0 ? (double)Wins / TotalMatches : 0;
        public double LossRate => TotalMatches > 0 ? (double)Losses / TotalMatches : 0;
        public double DrawRate => TotalMatches > 0 ? (double)Draws / TotalMatches : 0;
        public double AbandonmentRate => TotalMatches > 0 ? (double)(Forfeits + Disconnects) / TotalMatches : 0;

        public int MMRChange => CurrentMMR - StartingMMR;
        public string MMRGainLoss => MMRChange >= 0 ? $"+{MMRChange}" : MMRChange.ToString();

        public TimeSpan? AverageMatchDuration => TotalMatches > 0 && TotalPlayTime > TimeSpan.Zero ?
            TimeSpan.FromTicks(TotalPlayTime.Ticks / TotalMatches) : null;

        public bool IsActive => LastMatchAt.HasValue &&
            LastMatchAt.Value > DateTime.UtcNow.AddDays(-30);
    }
}