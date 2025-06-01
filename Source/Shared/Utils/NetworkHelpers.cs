using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using CelestialLeague.Shared.Enums;

namespace CelestialLeague.Shared.Utils
{
    public class NetworkHelpers
    {
        // connection methods
        public static async Task<int> GetPingAsync(string hostname)
        {
            try
            {
                using var ping = new Ping();
                var reply = await ping.SendPingAsync(hostname, NetworkConstants.PingTimeout);

                return reply.Status == IPStatus.Success ? (int)reply.RoundtripTime : -1;
            }
            catch
            {
                return -1;
            }
        }

        public static ConnectionQuality GetConnectionQuality(int ping)
        {
            return ping switch
            {
                < 0 => ConnectionQuality.Disconnected,
                <= 50 => ConnectionQuality.Excellent,
                <= 100 => ConnectionQuality.Good,
                <= 150 => ConnectionQuality.Fair,
                <= 250 => ConnectionQuality.Poor,
                _ => ConnectionQuality.VeryPoor
            };
        }

        // packet size validation methods
        public static bool IsValidPacketSized(byte[] data)
        {
            return data?.Length > 0 && data.Length <= NetworkConstants.MaxPacketSize;
        }

        public static bool IsValidMessageLength(string message)
        {
            return !string.IsNullOrEmpty(message) && Encoding.UTF8.GetByteCount(message) <= NetworkConstants.MaxMessageLength;
        }

        // rate limiting helpers
        public static string GenerateRateLimitKey(string clientId, string action)
        {
            return $"{clientId}:{action}:{DateTimeOffset.UtcNow:yyyyMMdd}";
        }

        public static bool IsRateLimited(int currentCount, int maxRequests)
        {
            return currentCount >= maxRequests;
        }

        // security helpers
        public static string GenerateSessionToken()
        {
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[32];
            rng.GetBytes(bytes);

            return Convert.ToBase64String(bytes);
        }

        public static string HashPassword(string password, string salt)
        {
            using var pbkdf2 = new Rfc2898DeriveBytes(password, Encoding.UTF8.GetBytes(salt), 10000, HashAlgorithmName.SHA256);
            var hash = pbkdf2.GetBytes(32);

            return Convert.ToBase64String(hash);
        }

        public static bool ValidateSessionToken(string token)
        {
            if (string.IsNullOrEmpty(token))
                return false;

            try
            {
                var bytes = Convert.FromBase64String(token);
                return bytes.Length == 32;
            }
            catch
            {
                return false;
            }
        }

        // endpoint validation
        public static bool IsValidIPAddress(string ipAddress)
        {
            return IPAddress.TryParse(ipAddress, out _);
        }

        public static bool IsValidPort(int port)
        {
            return port >= NetworkConstants.MinPort && port <= NetworkConstants.MaxPort;
        }

        public static bool IsValidEndpoint(string host, int port)
        {
            return !string.IsNullOrEmpty(host) && IsValidPort(port);
        }

        // protocol helpers
        public static byte[] CreatePacketHeader(PacketType type, int dataLength)
        {
            var header = new byte[NetworkConstants.PacketHeaderSize];
            header[0] = (byte)type;
            BitConverter.GetBytes(dataLength).CopyTo(header, 1);

            BitConverter.GetBytes(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()).CopyTo(header, 5);
            return header;
        }

        public static (PacketType type, int length, long timestamp) ParsePacketHeader(byte[] header)
        {
            if (header!.Length != NetworkConstants.PacketHeaderSize)
            {
                throw new ArgumentException("Invalid packet header size.");
            }

            var type = (PacketType)header[0];
            var length = BitConverter.ToInt32(header, 1);
            var timestamp = BitConverter.ToInt64(header, 5);

            return (type, length, timestamp);
        }

        // retry logic
        public static async Task<T> RetryAsync<T>(Func<Task<T>> operation, int maxRetries = 3, int delayMs = 1000)
        {
            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    return await operation();
                }
                catch when (i < maxRetries - 1)
                {
                    await Task.Delay(delayMs * i + 1); // exponential backoff
                }
            }

            return await operation();
        }

        // bandwith estimation
        public static double CalculateBandwith(long bytesTransferred, TimeSpan duration)
        {
            if (duration.TotalSeconds == 0)
                return 0;

            return bytesTransferred * 8 / duration.TotalSeconds;
        }

        // network state helpers
        public static bool IsNetworkAvailable()
        {
            return NetworkInterface.GetIsNetworkAvailable();
        }

        public static NetworkInterfaceType GetActiveNetworkType()
        {
            foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.OperationalStatus == OperationalStatus.Up &&
                ni.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                    return ni.NetworkInterfaceType;
            }

            return NetworkInterfaceType.Unknown;
        }

        public static bool ShouldCompress(byte[] data)
        {
            return data?.Length > NetworkConstants.CompressionThreshold;
        }

        // timeout helpers
        public static TimeSpan GetTimeoutForOperation(NetworkOperation operation)
        {
            return operation switch
            {
                NetworkOperation.Connect => TimeSpan.FromSeconds(10),
                NetworkOperation.Authenticate => TimeSpan.FromSeconds(5),
                NetworkOperation.Matchmaking => TimeSpan.FromMinutes(2),
                NetworkOperation.GameData => TimeSpan.FromSeconds(1),
                NetworkOperation.Chat => TimeSpan.FromSeconds(3),
                _ => TimeSpan.FromSeconds(5),
            };
        }

        // error handling 
        public static bool IsRetryableError(Exception ex)
        {
            return ex is TimeoutException or
            SocketException or
            HttpRequestException;
        }

        public static string GetFriendlyErrorMessage(Exception ex)
        {
            return ex switch
            {
                TimeoutException => "Connection timed out.",
                SocketException => "Network connection failed.",
                UnauthorizedAccessException => "Authentication failed.",
                _ => "An unexpected network error occured"
            };
        }
    }
}