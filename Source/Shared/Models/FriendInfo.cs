using CelestialLeague.Shared.Enums;

namespace CelestialLeague.Shared.Models
{
    public class FriendInfo
    {
        public required int PlayerId { get; set; }
        public required string Username { get; set; }
        public PlayerStatus Status { get; set; }
        public RankTier Rank { get; set; }
        public DateTime FriendsSince { get; set; }
        public DateTime LastSeen { get; set; }
    }
}
