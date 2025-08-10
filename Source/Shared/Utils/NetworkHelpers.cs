using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using CelestialLeague.Shared.Enum;

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
                var reply = await ping.SendPingAsync(hostname, Constants.Network.PingTimeoutMs);
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
                <= Constants.Network.GoodPingThresholdMs => ConnectionQuality.Excellent,
                <= Constants.Network.FairPingThresholdMs => ConnectionQuality.Good,
                <= Constants.Network.PoorPingThresholdMs => ConnectionQuality.Fair,
                <= (Constants.Network.PoorPingThresholdMs + 50) => ConnectionQuality.Poor,
                _ => ConnectionQuality.VeryPoor
            };
        }

        // packet size validation methods
        public static bool IsValidPacketSize(byte[] data)
        {
            return data?.Length > 0 && data.Length <= Constants.Network.MaxPacketSize;
        }

        public static bool IsValidMessageLength(string message)
        {
            return !string.IsNullOrEmpty(message) &&
                   Encoding.UTF8.GetByteCount(message) <= Constants.Network.MaxMessageLength;
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

        public static bool IsPositionUpdateRateLimited(DateTime lastUpdate)
        {
            var timeSince = (DateTime.UtcNow - lastUpdate).TotalMilliseconds;
            return timeSince < Constants.Network.PositionUpdateIntervalMs;
        }

        public static bool IsPacketFloodDetected(int packetsPerSecond, PacketType packetType)
        {
            return packetType switch
            {
                PacketType.PlayerPosition => packetsPerSecond > Constants.Network.MaxPositionUpdatesPerSecond,
                PacketType.GameEvent => packetsPerSecond > Constants.Network.MaxGameEventsPerSecond,
                PacketType.ChatMessage => packetsPerSecond > (Constants.Network.MaxMessagesPerMinute / 60),
                _ => packetsPerSecond > Constants.Network.MaxUdpPacketsPerSecond
            };
        }

        // endpoint validation
        public static bool IsValidIPAddress(string ipAddress)
        {
            return IPAddress.TryParse(ipAddress, out _);
        }

        public static bool IsValidPort(int port)
        {
            return port >= Constants.Network.MinPort && port <= Constants.Network.MaxPort;
        }

        public static bool IsValidEndpoint(string host, int port)
        {
            return !string.IsNullOrEmpty(host) && IsValidPort(port);
        }

        // protocol helpers
        public static byte[] CreatePacketHeader(PacketType type, int dataLength)
        {
            var header = new byte[Constants.Network.PacketHeaderSize];

            // magic header for packet validation
            var magicBytes = Encoding.ASCII.GetBytes(Constants.Network.MagicHeader);
            magicBytes.CopyTo(header, 0);

            // protocol version
            header[4] = Constants.Network.ProtocolVersion;

            // packet type
            header[5] = (byte)type;

            // data length
            BitConverter.GetBytes(dataLength).CopyTo(header, 6);

            // timestamp
            BitConverter.GetBytes(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()).CopyTo(header, 10);

            return header;
        }

        public static (PacketType type, int length, long timestamp, bool isValid) ParsePacketHeader(byte[] header)
        {
            if (header?.Length != Constants.Network.PacketHeaderSize)
            {
                return (PacketType.Error, 0, 0, false);
            }

            // validate magic header
            var magicBytes = new byte[4];
            Array.Copy(header, 0, magicBytes, 0, 4);
            var magic = Encoding.ASCII.GetString(magicBytes);

            if (magic != Constants.Network.MagicHeader)
            {
                return (PacketType.Error, 0, 0, false);
            }

            // validate protocol version
            if (header[4] != Constants.Network.ProtocolVersion)
            {
                return (PacketType.Error, 0, 0, false);
            }

            var type = (PacketType)header[5];
            var length = BitConverter.ToInt32(header, 6);
            var timestamp = BitConverter.ToInt64(header, 10);

            // validate packet size
            if (length > Constants.Network.MaxPacketSize || length < 0)
            {
                return (PacketType.Error, 0, 0, false);
            }

            return (type, length, timestamp, true);
        }

        // retry logic
        public static async Task<T> RetryAsync<T>(Func<Task<T>> operation, int maxRetries = Constants.Network.MaxReconnectAttempts, int baseDelayMs = Constants.Network.ConnectionRetryDelayMs)
        {
            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    return await operation();
                }
                catch when (i < maxRetries - 1)
                {
                    await Task.Delay(baseDelayMs * (i + 1)); // exponential backoff
                }
            }
            return await operation();
        }

        // bandwidth estimation
        public static double CalculateBandwidth(long bytesTransferred, TimeSpan duration)
        {
            if (duration.TotalSeconds == 0)
                return 0;
            return bytesTransferred * 8 / duration.TotalSeconds; // bits per second
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
            return Constants.Network.EnableCompression &&
                   data?.Length > Constants.Network.CompressionThreshold;
        }

        // buffer management
        public static int CalculateOptimalBufferSize(int dataSize)
        {
            if (dataSize <= Constants.Network.MinBufferSize)
                return Constants.Network.MinBufferSize;

            if (dataSize >= Constants.Network.MaxBufferSize)
                return Constants.Network.MaxBufferSize;

            // round up to next power of 2
            int bufferSize = Constants.Network.MinBufferSize;
            while (bufferSize < dataSize)
            {
                bufferSize *= Constants.Network.BufferGrowthFactor;
            }

            return Math.Min(bufferSize, Constants.Network.MaxBufferSize);
        }

        // timeout helpers
        public static TimeSpan GetTimeoutForOperation(NetworkOperation operation)
        {
            return operation switch
            {
                NetworkOperation.Connect => TimeSpan.FromMilliseconds(Constants.Network.ConnectionTimeoutMs),
                NetworkOperation.Authenticate => TimeSpan.FromMilliseconds(Constants.Network.SocketTimeoutMs),
                NetworkOperation.Matchmaking => TimeSpan.FromMilliseconds(Constants.Network.MatchmakingTimeoutMs),
                NetworkOperation.GameData => TimeSpan.FromMilliseconds(Constants.Network.PingTimeoutMs),
                NetworkOperation.Chat => TimeSpan.FromMilliseconds(Constants.Network.SocketTimeoutMs / 2),
                _ => TimeSpan.FromMilliseconds(Constants.Network.SocketTimeoutMs),
            };
        }

        // connection quality assessment
        public static bool IsConnectionStable(int ping, int packetLoss)
        {
            return ping <= Constants.Network.FairPingThresholdMs &&
                   packetLoss <= Constants.Network.PacketLossThresholdPercent;
        }

        public static bool IsHighLatencyConnection(int ping)
        {
            return ping > Constants.Network.PoorPingThresholdMs;
        }

        // security helpers
        public static bool IsSuspiciousActivity(int packetsPerSecond)
        {
            return packetsPerSecond > Constants.Network.SuspiciousActivityThreshold;
        }

        public static bool ShouldAutoDisconnect(int packetsPerSecond)
        {
            return packetsPerSecond > Constants.Network.AutoDisconnectThreshold;
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
                TimeoutException => "connection timed out",
                SocketException => "network connection failed",
                UnauthorizedAccessException => "authentication failed",
                _ => "an unexpected network error occurred"
            };
        }

        // packet queue management
        public static bool IsPacketQueueOverloaded(int queueSize)
        {
            return queueSize > Constants.Network.PacketQueueWarningThreshold;
        }

        public static bool ShouldDropPackets(int queueSize)
        {
            return queueSize > Constants.Network.PacketDropThreshold;
        }

        // network congestion detection
        public static bool IsNetworkCongested(double utilizationPercent)
        {
            return utilizationPercent > Constants.Network.NetworkCongestionThreshold;
        }
    }
}

