namespace CelestialLeague.Shared.Utils
{
    public class Validation
    {
        public static bool IsValidUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return false;

            if (username.Length < GameConstants.MinUsernameLength ||
                username.Length > GameConstants.MaxUsernameLength)
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
    }
}