namespace CelestialLeague.Shared.Utils
{
    public class TimeHelpers
    {
        // time conversion
        public static long ToUnixTimestamp(DateTime dateTime)
        {
            return (long)(dateTime - new DateTime(1970, 1, 1)).TotalSeconds;
        }

        public static DateTime FromUnixTimestamp(long unixTimestamp)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(unixTimestamp);
        }

        public static string ToRelativeTime(DateTime dateTime)
        {
            var timeSpan = DateTime.UtcNow - dateTime;
            
            return timeSpan switch
            {
                { TotalSeconds: < 60 } => "just now",
                { TotalMinutes: < 60 } => $"{(int)timeSpan.TotalMinutes}m ago",
                { TotalHours: < 24 } => $"{(int)timeSpan.TotalHours}h ago",
                { TotalDays: < 7 } => $"{(int)timeSpan.TotalDays}d ago",
                { TotalDays: < 30 } => $"{(int)(timeSpan.TotalDays / 7)}w ago",
                { TotalDays: < 365 } => $"{(int)(timeSpan.TotalDays / 30)}mo ago",
                _ => $"{(int)(timeSpan.TotalDays / 365)}y ago"
            };
        }

        public static TimeSpan ParseDuration(string duration)
        {
            // Parse formats like "1h30m", "45s", "2d", "1h", "30m"
            if (string.IsNullOrWhiteSpace(duration))
                return TimeSpan.Zero;

            var totalSeconds = 0;
            var currentNumber = "";
            
            foreach (char c in duration.ToLower())
            {
                if (char.IsDigit(c))
                {
                    currentNumber += c;
                }
                else if (int.TryParse(currentNumber, out int value))
                {
                    totalSeconds += c switch
                    {
                        's' => value,
                        'm' => value * 60,
                        'h' => value * 3600,
                        'd' => value * 86400,
                        _ => 0
                    };
                    currentNumber = "";
                }
            }
            
            return TimeSpan.FromSeconds(totalSeconds);
        }

        // formatting
        public static string FormatDuration(TimeSpan duration)
        {
            if (duration.TotalDays >= 1)
                return $"{(int)duration.TotalDays}d {duration.Hours}h {duration.Minutes}m";
            if (duration.TotalHours >= 1)
                return $"{duration.Hours}h {duration.Minutes}m";
            if (duration.TotalMinutes >= 1)
                return $"{duration.Minutes}m {duration.Seconds}s";
            return $"{duration.Seconds}s";
        }

        public static string FormatMatchTime(TimeSpan matchTime)
        {
            // MM:SS for matches
            return $"{(int)matchTime.TotalMinutes:D2}:{matchTime.Seconds:D2}";
        }

        public static string FormatCountdown(TimeSpan remaining)
        {
            if (remaining.TotalDays >= 1)
                return $"{(int)remaining.TotalDays}d {remaining.Hours:D2}h {remaining.Minutes:D2}m";
            if (remaining.TotalHours >= 1)
                return $"{remaining.Hours:D2}h {remaining.Minutes:D2}m {remaining.Seconds:D2}s";
            return $"{remaining.Minutes:D2}m {remaining.Seconds:D2}s";
        }

        // validation
        public static bool IsValidTimestamp(long timestamp)
        {
            return timestamp >= 0 && timestamp <= 4102444800;
        }

        public static bool IsRecentTimestamp(long timestamp, TimeSpan maxAge)
        {
            var timestampDate = FromUnixTimestamp(timestamp);
            return DateTime.UtcNow - timestampDate <= maxAge;
        }

        public static bool IsInTimeWindow(DateTime time, TimeSpan window)
        {
            var now = DateTime.UtcNow;
            return Math.Abs((now - time).TotalMilliseconds) <= window.TotalMilliseconds;
        }

        // game time
        public static DateTime GetSeasonStart(int seasonNumber)
        {
            if (seasonNumber < 1) return Constants.Game.FirstSeasonStart;
            return Constants.Game.FirstSeasonStart.AddTicks((seasonNumber - 1) * Constants.Game.SeasonDuration.Ticks);
        }

        public static DateTime GetSeasonEnd(int seasonNumber)
        {
            return GetSeasonStart(seasonNumber).Add(Constants.Game.SeasonDuration);
        }

        public static int GetCurrentSeason()
        {
            var elapsed = DateTime.UtcNow - Constants.Game.FirstSeasonStart;
            return Math.Max(1, (int)(elapsed.Ticks / Constants.Game.SeasonDuration.Ticks) + 1);
        }

        public static TimeSpan GetTimeUntilSeasonEnd()
        {
            var currentSeason = GetCurrentSeason();
            var seasonEnd = GetSeasonEnd(currentSeason);
            return seasonEnd - DateTime.UtcNow;
        }
    }
}

