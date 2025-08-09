using System.Text.RegularExpressions;
using CelestialLeague.Shared.Enums;

namespace CelestialLeague.Shared.Utils
{
    public class StringHelpers
    {
        //text processing
        public static string Truncate(string text, int maxLength)
        {
            return text.Length <= maxLength ? text : text.Substring(0, maxLength);
        }
        
        public static string RemoveSpecialCharacters(string text)
        {
            return Regex.Replace(text, @"[^a-zA-Z0-9\s]", "");
        }
        
        public static string ToTitleCase(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(text.ToLower());
        }
        
        public static bool ContainsOnlyAlphanumeric(string text)
        {
            return !string.IsNullOrEmpty(text) && text.All(char.IsLetterOrDigit);
        }

        // chat utilities
        public static string[] ExtractMentions(string message)
        {
            var mentions = Regex.Matches(message, @"@(\w+)")
                .Cast<Match>()
                .Select(m => m.Groups[1].Value)
                .ToArray();
            return mentions;
        }
        
        public static string HighlightMentions(string message, string currentUser)
        {
            return Regex.Replace(message, $@"@{currentUser}\b", $"**@{currentUser}**", RegexOptions.IgnoreCase);
        }

        // formatting
        public static string FormatPlayerName(string name, RankTier rank)
        {
            var rankPrefix = rank switch
            {
                RankTier.Bronze => "[B]",
                RankTier.Silver => "[S]",
                RankTier.Gold => "[G]",
                RankTier.Platinum => "[P]",
                RankTier.Diamond => "[D]",
                _ => ""
            };
            return $"{rankPrefix} {name}";
        }
        
        public static string FormatNumber(int number)
        {
            return number switch
            {
                >= 1000000 => $"{number / 1000000.0:F1}M",
                >= 1000 => $"{number / 1000.0:F1}K",
                _ => number.ToString()
            };
        }
        
        public static string FormatPercentage(float percentage)
        {
            return $"{percentage:F1}%";
        }

        // basic text utilities
        public static bool ContainsBannedWords(string text)
        {
            return ChatConstants.ProfanityList.Any(word => text.Contains(word, StringComparison.OrdinalIgnoreCase));
        }
        
        public static int GetWordCount(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return 0;
            return text.Split(new char[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;
        }
    }
}
