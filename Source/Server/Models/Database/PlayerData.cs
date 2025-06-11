using System.Diagnostics.CodeAnalysis;
using CelestialLeague.Shared.Enums;

namespace CelestialLeague.Server.Models
{
    public class Player
    {
        public int Id { get; set; }
        public required string Username { get; set; }
        public required string PasswordHash { get; set; }
        public required string PasswordSalt { get; set; }

        // timestamps
        public required DateTime CreatedAt { get; set; }
        public required DateTime LastSeen { get; set; }

        // rating
        // public int MMR { get; set; } = GameConstants.StartingMMR;
        // public RankTier Rank { get; set; } = RankTier.Bronze;

        // stats
        // public int TotalMatches { get; set; } = 0;
        // public int Wins { get; set; } = 0;
        // public int Losses { get; set; } = 0;
        // public TimeSpan BestTime { get; set; } = TimeSpan.Zero;
        // public int WinStreak { get; set; } = 0;
        // public int BestWinStreak { get; set; } = 0;

        // status
        public PlayerStatus PlayerStatus { get; set; } = PlayerStatus.Offline;

        // moderation
        // public DateTime? BanExpires { get; set; }
        // public DateTime? MuteExpires { get; set; }
        // public DateTime? PenaltyExpires { get; set; }
        public UserRole UserRole { get; set; } = UserRole.None;

        // navigation properties for EF relationships
        // public virtual ICollection<Friendship> SentFriendRequests { get; } = new List<Friendship>();
        // public virtual ICollection<Friendship> ReceivedFriendRequests { get; } = new List<Friendship>();

        // constructor
        [SetsRequiredMembers]
        public Player(string username, string passwordHash, string passwordSalt, DateTime createdAt)
        {
            Username = username;
            PasswordHash = passwordHash;
            PasswordSalt = passwordSalt;

            CreatedAt = createdAt;
            LastSeen = DateTime.UtcNow;

            PlayerStatus = PlayerStatus.Offline;
            UserRole = UserRole.None;
        }
    }
}
