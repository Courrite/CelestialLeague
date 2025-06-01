using System.Numerics;
using CelestialLeague.Shared.Enums;

namespace CelestialLeague.Shared.Utils
{
    public class MathHelpers
    {
        // basic
        public static float Clamp(float value, float min, float max)
        {
            return Math.Max(min, Math.Min(max, value));
        }
        
        public static int Clamp(int value, int min, int max)
        {
            return Math.Max(min, Math.Min(max, value));
        }
        
        public static float Lerp(float a, float b, float t)
        {
            return a + (b - a) * t;
        }
        
        public static bool Approximately(float a, float b, float tolerance = 0.001f)
        {
            return Math.Abs(a - b) < tolerance;
        }

        // vector
        public static float Distance(Vector2 a, Vector2 b)
        {
            return (float)Math.Sqrt(Math.Pow(b.X - a.X, 2) + Math.Pow(b.Y - a.Y, 2));
        }
        
        public static Vector2 Normalize(Vector2 vector)
        {
            float length = (float)Math.Sqrt(vector.X * vector.X + vector.Y * vector.Y);
            if (length == 0) return Vector2.Zero;
            return new Vector2(vector.X / length, vector.Y / length);
        }
        
        public static float Dot(Vector2 a, Vector2 b)
        {
            return a.X * b.X + a.Y * b.Y;
        }
        
        public static Vector2 Rotate(Vector2 vector, float angle)
        {
            float cos = (float)Math.Cos(angle);
            float sin = (float)Math.Sin(angle);
            return new Vector2(
                vector.X * cos - vector.Y * sin,
                vector.X * sin + vector.Y * cos
            );
        }

        // game math
        public static int CalculateEloChange(int playerRating, int opponentRating, bool playerWon)
        {
            const int K = 32;
            
            float expectedScore = 1.0f / (1.0f + (float)Math.Pow(10, (opponentRating - playerRating) / 400.0));
            float actualScore = playerWon ? 1.0f : 0.0f;
            
            return (int)Math.Round(K * (actualScore - expectedScore));
        }
        
        public static float CalculateMatchmakingWeight(int ratingDiff, int timeDiff)
        {
            float ratingWeight = 1.0f / (1.0f + Math.Abs(ratingDiff) / 100.0f);
            float timeWeight = Math.Min(1.0f, timeDiff / 60.0f);
            
            return ratingWeight * (1.0f + timeWeight);
        }
        
        public static int CalculateExperience(MatchResult result, int matchDuration)
        {
            int baseXP = result switch
            {
                MatchResult.Victory => 100,
                MatchResult.Defeat => 50,
                MatchResult.Draw => 75,
                MatchResult.Forfeit => 10,
                MatchResult.Disconnect => 0,
                _ => 0
            };
            
            int durationBonus = Math.Min(matchDuration / 60, 10) * 5;
            
            return baseXP + durationBonus;
        }

        // statistics
        public static float Average(IEnumerable<float> values)
        {
            if (!values.Any()) return 0f;
            return values.Sum() / values.Count();
        }
        
        public static float StandardDeviation(IEnumerable<float> values)
        {
            if (!values.Any()) return 0f;
            
            float average = Average(values);
            float sumOfSquares = values.Sum(x => (x - average) * (x - average));
            return (float)Math.Sqrt(sumOfSquares / values.Count());
        }
        
        public static int Percentile(IEnumerable<int> values, float percentile)
        {
            if (!values.Any()) return 0;
            
            var sorted = values.OrderBy(x => x).ToArray();
            int index = (int)Math.Ceiling(percentile / 100.0 * sorted.Length) - 1;
            index = Clamp(index, 0, sorted.Length - 1);
            
            return sorted[index];
        }
    }
}