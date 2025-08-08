namespace CelestialLeague.Shared.Enums
{
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
}