using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using CelestialLeague.Shared.Enums;

namespace CelestialLeague.Shared.Models
{
    public class GameMatch
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        // players
        public int Player1Id { get; set; }
        public int Player2Id { get; set; }

        // game data
        public TimeSpan? Player1Time { get; set; }
        public TimeSpan? Player2Time { get; set; }

        public TimeSpan? Player1Deaths { get; set; }
        public TimeSpan? Player2Deaths { get; set; }

        // mmr tracking
        public int Player1MMRBefore { get; set; }
        public int Player2MMRBefore { get; set; }
        public int Player1MMRAfter { get; set; }
        public int Player2MMRAfter { get; set; }

        // match state and results
        public MatchState State { get; set; } = MatchState.Created;
        public int WinnerId { get; set; }
        public int LoserId { get; set; }
        public MatchResult LossReason { get; set; }

        // timing
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan Duration => EndTime - StartTime;

        // match outcome
        public MatchOutcome Outcome { get; set; } = MatchOutcome.InProgress;
        public string? Message { get; set; }

        // metadata
        public string ServerVersion { get; set; } = "1.0.0";
        public string GameVersion { get; set; } = string.Empty;
        public string Region { get; set; } = "Unknown";
    }
}