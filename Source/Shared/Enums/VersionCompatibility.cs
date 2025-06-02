 public enum VersionCompatibility
    {
        Perfect, // exact match with current version
        Compatible, // older but still supported
        UpdateRequired, // too old, update recommended
        ForceUpdateRequired, // critically old, must update
        TooNew, // newer than server supports
        InvalidFormat, // invalid version string
        Unknown // unexpected state
    }