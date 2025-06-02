 public enum VersionCompatibility
    {
        Perfect,                // Exact match with current version
        Compatible,             // Older but still supported
        UpdateRequired,         // Too old, update recommended
        ForceUpdateRequired,    // Critically old, must update
        TooNew,                 // Newer than server supports
        InvalidFormat,          // Invalid version string
        Unknown                 // Unexpected state
    }