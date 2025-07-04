using System.Text.RegularExpressions;
using CelestialLeague.Shared.Enums;

namespace CelestialLeague.Shared.Utils
{
    public static class VersionUtils
    {
        public static bool IsValidVersionFormat(string version)
        {
            if (string.IsNullOrWhiteSpace(version))
                return false;
                
            return Regex.IsMatch(version, VersionConstants.VERSION_PATTERN);
        }
        
        public static Version ParseVersion(string versionString)
        {
            if (!IsValidVersionFormat(versionString))
                throw new ArgumentException($"Invalid version format: {versionString}");
                
            return new Version(versionString);
        }
        
        public static VersionCompatibility CheckClientCompatibility(string clientVersion)
        {
            if (!IsValidVersionFormat(clientVersion))
                return VersionCompatibility.InvalidFormat;
            
            var client = ParseVersion(clientVersion);
            var minSupported = ParseVersion(VersionConstants.MIN_SUPPORTED_CLIENT_VERSION);
            var current = ParseVersion(VersionConstants.CURRENT_CLIENT_VERSION);
            var forceUpdateThreshold = ParseVersion(VersionConstants.FORCE_UPDATE_THRESHOLD);
            
            // too old - force update required
            if (client < forceUpdateThreshold)
                return VersionCompatibility.ForceUpdateRequired;
            
            // below minimum supported - update required
            if (client < minSupported)
                return VersionCompatibility.UpdateRequired;
            
            // future version - not supported
            if (client > current)
                return VersionCompatibility.TooNew;
            
            // perfect match
            if (client == current)
                return VersionCompatibility.Perfect;
            
            // older but still supported
            if (client >= minSupported && client < current)
                return VersionCompatibility.Compatible;
            
            return VersionCompatibility.Unknown;
        }
        
        public static bool IsClientVersionSupported(string clientVersion)
        {
            var compatibility = CheckClientCompatibility(clientVersion);
            return compatibility == VersionCompatibility.Perfect || 
                   compatibility == VersionCompatibility.Compatible;
        }
        
        public static string GetVersionMessage(VersionCompatibility compatibility)
        {
            return compatibility switch
            {
                VersionCompatibility.Perfect => "Client version is up to date",
                VersionCompatibility.Compatible => "Client version is supported but outdated",
                VersionCompatibility.UpdateRequired => "Client version is too old and must be updated",
                VersionCompatibility.ForceUpdateRequired => "Client version is critically outdated and requires immediate update",
                VersionCompatibility.TooNew => "Client version is newer than server supports",
                VersionCompatibility.InvalidFormat => "Client version format is invalid",
                _ => "Unknown version compatibility status"
            };
        }
    }
}
