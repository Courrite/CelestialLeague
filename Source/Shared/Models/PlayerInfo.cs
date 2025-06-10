using CelestialLeague.Shared.Enums;

namespace CelestialLeague.Shared.Models
{
    public class PlayerInfo
    {
        // credential and profile
        public required int Id { get; set; }
        public required string Username { get; set; }
        public required DateTime CreatedAt { get; set; }
        public required DateTime LastSeen { get; set; }

        // rating
        public int MMR { get; set; } = 0;
        public RankTier Rank { get; set; } = 0;

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
        public List<(int PlayerId, DateTime FriendsSince)> Friends { get; set; } = new();
        public List<(int PlayerId, DateTime BlockedSince)> Blocked { get; set; } = new();

        // moderation
        public DateTime? BanExpires { get; set; }
        public DateTime? MuteExpires { get; set; }
        public DateTime? PenaltyExpires { get; set; }
        public UserRole UserRole { get; set; } 
    }
}