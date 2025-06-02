namespace CelestialLeague.Shared.Constants
{
    public static class VersionConstants
    {
        // current versions
        public const string CURRENT_CLIENT_VERSION = "1.0.0";
        public const string CURRENT_SERVER_VERSION = "1.0.0";
        
        // minimum supported versions
        public const string MIN_SUPPORTED_CLIENT_VERSION = "1.0.0";
        public const string MIN_SUPPORTED_SERVER_VERSION = "1.0.0";
        
        // version that requires forced update
        public const string FORCE_UPDATE_THRESHOLD = "0.9.0";
        
        // version format validation regex
        public const string VERSION_PATTERN = @"^\d+\.\d+\.\d+$";
    }
}
