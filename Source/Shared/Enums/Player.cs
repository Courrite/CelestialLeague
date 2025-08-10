namespace CelestialLeague.Shared.Enum
{
    public enum PlayerMatchState
    {
        NotReady = 0,
        Ready = 1,
        Loading = 2,
        InGame = 3,
        Disconnected = 4
    }

    [Flags]
    public enum PlayerStateFlags : byte
    {
        None = 0,
        OnGround = 1 << 0, // 0000 0001 = 1
        Dashing = 1 << 1, // 0000 0010 = 2
        Climbing = 1 << 2, // 0000 0100 = 4
        WallSliding = 1 << 3, // 0000 1000 = 8
        Dead = 1 << 4, // 0001 0000 = 16
        Holding = 1 << 5, // 0010 0000 = 32
        Ducking = 1 << 6, // 0100 0000 = 64
        Reserved = 1 << 7 // for future use
    }

    public enum PlayerStatus
    {
        Offline = 1, // player is not in game or doesn't have his mod on
        Online = 2, // player is online and has his mod on
        InQueue = 3, // player is online and is waiting for a match
        Playing = 4, // player is in an active match
        Spectating = 5, // player is spectating a match
    }

    public enum RankTier
    {
        Bronze = 1,
        Silver = 2,
        Gold = 3,
        Platinum = 4,
        Diamond = 5,
        Master = 6,
        Grandmaster = 7
    }

    [Flags]
    public enum UserRole
    {
        None = 0,
        Admin = 1 << 0,
        Moderator = 1 << 1,
        Developer = 1 << 2,
        Owner = 1 << 3,
    }

    public enum FriendshipStatus
    {
        Pending = 0,
        Accepted = 1,
        Blocked = 2,
        Declined = 3
    }
}