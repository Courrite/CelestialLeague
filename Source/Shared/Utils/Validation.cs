using System.Text.RegularExpressions;
using CelestialLeague.Shared.Enum;

namespace CelestialLeague.Shared.Utils
{
    public class Validation
    {
        private static readonly Regex EmailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");

        public static bool IsValidUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return false;

            if (username.Length < Constants.Game.MinUsernameLength ||
                username.Length > Constants.Game.MaxUsernameLength)
                return false;

            if (username.Contains(' '))
                return false;

            if (!username.All(c => char.IsLetterOrDigit(c) || c == '_' || c == '-'))
                return false;

            if (!char.IsLetterOrDigit(username[0]))
                return false;

            if (!char.IsLetterOrDigit(username[username.Length - 1]))
                return false;

            for (int i = 0; i < username.Length - 1; i++)
            {
                if ((username[i] == '_' || username[i] == '-') &&
                    (username[i + 1] == '_' || username[i + 1] == '-'))
                    return false;
            }

            return true;
        }

        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            if (!EmailRegex.IsMatch(email))
                return false;

            return true;
        }

        public static bool IsValidPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return false;

            if (password.Length < Constants.Security.MinPasswordLength ||
            password.Length > Constants.Security.MaxPasswordLength)
                return false;

            return true;
        }

        // game data validation
        public static bool IsValidMMR(int mmr)
        {
            if (mmr < Constants.Game.MinMMR || mmr > Constants.Game.MaxMMR)
                return false;

            return true;
        }

        public static bool IsValidMatchResult(MatchResult result)
        {
            if (!System.Enum.IsDefined(typeof(MatchResult), result))
                return false;

            return true;
        }

        // network data validation
        public static bool IsValidPacketType(PacketType type)
        {
            if (!System.Enum.IsDefined(typeof(PacketType), type))
                return false;

            return true;
        }

        public static bool IsValidPlayerStatus(PlayerStatus status)
        {
            if (!System.Enum.IsDefined(typeof(PlayerStatus), status))
                return false;

            return true;
        }

        // range validation
        public static bool IsInRange(int value, int min, int max)
        {
            return value >= min && value <= max;
        }

        public static bool IsInRange(float value, float min, float max)
        {
            return value >= min && value <= max;
        }

        // collection validation
        public static bool IsValidCollection<T>(IEnumerable<T> collection, int maxCount)
        {
            if (collection == null || collection.Count() > maxCount)
                return false;

            return true;
        }

        public static bool HasValidElement<T>(IEnumerable<T> collection, Func<T, bool> validator)
        {
            if (collection == null || validator == null)
                return false;

            return collection.All(validator);
        }
    }

}