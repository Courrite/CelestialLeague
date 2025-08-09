<<<<<<< HEAD
using CelestialLeague.Shared.Enums;

namespace CelestialLeague.Server.Models
{
    public class Friendship
    {
        public int Id { get; set; }

        public int RequesterId { get; set; }
        public int ReceiverId { get; set; }

        public FriendshipStatus Status { get; set; }
        public DateTime RequestedAt { get; set; }
        public DateTime? AcceptedAt { get; set; }
        public DateTime? BlockedAt { get; set; }

        // Navigation properties for EF relationships
        public required virtual Player Requester { get; set; }
        public required virtual Player Receiver { get; set; }

        public bool IsPending => Status == FriendshipStatus.Pending;
        public bool IsAccepted => Status == FriendshipStatus.Accepted;
        public bool IsBlocked => Status == FriendshipStatus.Blocked;
    }
=======
using CelestialLeague.Shared.Enums;

namespace CelestialLeague.Server.Models
{
    public class Friendship
    {
        public int Id { get; set; }

        public int RequesterId { get; set; }
        public int ReceiverId { get; set; }

        public FriendshipStatus Status { get; set; }
        public DateTime RequestedAt { get; set; }
        public DateTime? AcceptedAt { get; set; }
        public DateTime? BlockedAt { get; set; }

        // Navigation properties for EF relationships
        public required virtual Player Requester { get; set; }
        public required virtual Player Receiver { get; set; }

        public bool IsPending => Status == FriendshipStatus.Pending;
        public bool IsAccepted => Status == FriendshipStatus.Accepted;
        public bool IsBlocked => Status == FriendshipStatus.Blocked;
    }
>>>>>>> 48bc47b13401bb7e2dfc20bc611c767893bc8e52
}