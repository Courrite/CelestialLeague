using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace CelestialLeague.Shared.Utils
{
    public class SecurityHelpers
    {
        private static readonly Dictionary<string, List<DateTime>> RateLimitTracker = new();
        private static readonly object RateLimitLock = new object();
        private static readonly string[] MaliciousPatterns = {
             "<script",
            "</script>",
            "javascript:",
            "vbscript:",
            "onload=",
            "onerror=",
            "onclick=",
            "onmouseover=",
            "onfocus=",
            "onblur=",
            "onchange=",
            "onsubmit=",
            "&& ",
            "|| ",
            "; ",
            "| ",
            "`",
            "$(",
            "${",
            "cmd.exe",
            "powershell",
            "/bin/sh",
            "/bin/bash",
             "../",
            "..\\",
            "%2e%2e",
            "%252e%252e",
            "/etc/passwd",
            "/windows/system32",
            "drop table",
            "delete from",
            "insert into",
            "update set",
            "union select",
            "exec(",
            "execute(",
            "--",
            "/*",
            "*/"
        };

        // encryption
        public static string Encrypt(string plainText, string key)
        {
            using var aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes(key.PadRight(32).Substring(0, 32));
            aes.GenerateIV();

            using var encryptor = aes.CreateEncryptor();
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            var encryptedBytes = encryptor.TransformFinalBlock(plainTextBytes, 0, plainTextBytes.Length);

            var result = new byte[aes.IV.Length + encryptedBytes.Length];
            Array.Copy(aes.IV, 0, result, 0, aes.IV.Length);
            Array.Copy(encryptedBytes, 0, result, aes.IV.Length, encryptedBytes.Length);

            return Convert.ToBase64String(result);
        }

        public static string Decrypt(string ciphertext, string key)
        {
            var cipherBytes = Convert.FromBase64String(ciphertext);

            using var aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes(key.PadRight(32).Substring(0, 32));

            var iv = new byte[16];
            var encrypted = new byte[cipherBytes.Length - 16];
            Array.Copy(cipherBytes, 0, iv, 0, 16);
            Array.Copy(cipherBytes, 16, encrypted, 0, encrypted.Length);
            aes.IV = iv;

            using var decryptor = aes.CreateDecryptor();
            var decryptedBytes = decryptor.TransformFinalBlock(encrypted, 0, encrypted.Length);

            return Encoding.UTF8.GetString(decryptedBytes);
        }

        public static string GenerateSalt()
        {
            var salt = new byte[Constants.Security.PasswordSaltLength];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(salt);

            return Convert.ToBase64String(salt);
        }

        // hashing
        public static string HashSHA256(string input)
        {
            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));

            return Convert.ToBase64String(hashBytes);
        }

        public static string HashPassword(string password, string salt)
        {
            using var pbkdf2 = new Rfc2898DeriveBytes(password, Convert.FromBase64String(salt), Constants.Security.PasswordHashIterations, HashAlgorithmName.SHA256);
            var hash = pbkdf2.GetBytes(Constants.Security.PasswordHashLength);

            return Convert.ToBase64String(hash);
        }

        public static bool VerifyPassword(string password, string hash, string salt)
        {
            var computedHash = HashPassword(password, salt);

            return hash == computedHash;
        }

        // token management
        public static string GenerateToken(int length = Constants.Security.SecureTokenLength)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_";
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[length];
            rng.GetBytes(bytes);

            var result = new StringBuilder(length);
            for (int i = 0; i < length; i++)
            {
                result.Append(chars[bytes[i] % chars.Length]);
            }
            return result.ToString();
        }

        // rate limiting
        public static bool CheckRateLimit(string identifier, int maxRequests, TimeSpan window)
        {
            lock (RateLimitLock)
            {
                var now = DateTime.UtcNow;
                var windowStart = now - window;

                if (!RateLimitTracker.ContainsKey(identifier))
                    RateLimitTracker[identifier] = new List<DateTime>();

                var requests = RateLimitTracker[identifier];

                requests.RemoveAll(time => time < windowStart);

                return requests.Count < maxRequests;
            }
        }

        public static bool CheckLoginRateLimit(string identifier)
        {
            return CheckRateLimit(identifier, Constants.Security.MaxLoginAttemptsPerMinute, TimeSpan.FromMinutes(1));
        }

        public static bool CheckMatchmakingRateLimit(string identifier)
        {
            return CheckRateLimit(identifier, Constants.Security.MaxMatchmakingRequestsPerMinute, TimeSpan.FromMinutes(1));
        }

        public static void RecordRequest(string identifier)
        {
            lock (RateLimitLock)
            {
                var now = DateTime.UtcNow;

                if (!RateLimitTracker.ContainsKey(identifier))
                    RateLimitTracker[identifier] = new List<DateTime>();

                RateLimitTracker[identifier].Add(now);

                CleanupOldEntries();
            }
        }

        private static void CleanupOldEntries()
        {
            var cutoff = DateTime.UtcNow.AddHours(-1);
            var keysToRemove = new List<string>();

            foreach (var kvp in RateLimitTracker)
            {
                kvp.Value.RemoveAll(time => time < cutoff);

                if (kvp.Value.Count == 0)
                    keysToRemove.Add(kvp.Key);
            }

            foreach (var key in keysToRemove)
                RateLimitTracker.Remove(key);
        }

        // input sanitization
        public static string SanitizeInput(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            var sanitized = input;
            foreach (var pattern in MaliciousPatterns)
            {
                sanitized = sanitized.Replace(pattern, "", StringComparison.OrdinalIgnoreCase);
            }

            sanitized = Regex.Replace(sanitized, @"[<>""'%;()&+]", "");

            sanitized = sanitized.Trim();

            if (sanitized.Length > Constants.Security.MaxReportReasonLength)
                sanitized = sanitized.Substring(0, Constants.Security.MaxReportReasonLength);

            return sanitized;
        }

        public static bool ContainsMaliciousContent(string input)
        {
            if (string.IsNullOrEmpty(input))
                return false;

            var lowerInput = input.ToLower();

            foreach (var pattern in MaliciousPatterns)
            {
                if (lowerInput.Contains(pattern.ToLower()))
                    return true;
            }

            var specialCharCount = input.Count(c => "!@#$%^&*(){}[]|\\:;\"'<>?/~`".Contains(c));
            if (specialCharCount > input.Length * 0.3)
                return true;

            return false;
        }

        public static string EscapeHtml(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            return input
                .Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;")
                .Replace("'", "&#x27;");
        }

    }

}