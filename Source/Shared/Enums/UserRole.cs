namespace CelestialLeague.Shared.Enums
{
    [Flags]
    public enum UserRole
    {
        None = 0,
        Admin = 1 << 0,
        Moderator = 1 << 1,
        Developer = 1 << 2,
        Owner = 1 << 3,
    }
}