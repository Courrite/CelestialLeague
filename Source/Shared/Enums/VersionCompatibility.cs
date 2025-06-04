namespace CelestialLeague.Shared.Enums
{
    public enum VersionCompatibility
    {
        Perfect = 1, // exact match with current version
        Compatible = 2, // older but still supported
        UpdateRequired = 3, // too old, update recommended
        ForceUpdateRequired = 4, // critically old, must update
        TooNew = 5, // newer than server supports
        InvalidFormat = 6, // invalid version string
        Unknown = 7 // unexpected state
    }
}