using System.Net.Sockets;
using CelestialLeague.Shared.Enum;
using CelestialLeague.Shared.Packets;
using CelestialLeague.Shared.Utils;
using Celeste.Mod;
using System;
using System.Threading.Tasks;
using System.IO;
using System.Threading;

namespace CelestialLeague.Client.Networking
{
    public class NetworkClient : IDisposable
    {
        private readonly object _sendLock = new object();
        private readonly SemaphoreSlim _sendSemaphore = new SemaphoreSlim(1, 1);
        private bool _disposed;

        public int PacketsSent { get; private set; }
        public int PacketsReceived { get; private set; }
        public int SendErrors { get; private set; }
        public int ReceiveErrors { get; private set; }
        public DateTime LastActivity { get; private set; } = DateTime.UtcNow;

        public NetworkClient()
        {
            Logger.Debug("Celestial League", "NetworkClient initialized");
        }

        public async Task<bool> SendPacketAsync<T>(T packet, NetworkStream stream) where T : BasePacket
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(NetworkClient));

            if (stream == null || !stream.CanWrite)
            {
                Logger.Warn("Celestial League", "Cannot send packet: Stream is null or not writable");
                return false;
            }

            try
            {
                var packetData = Serialization.SerializePacket(packet);

                if (packetData.Length > Shared.Constants.Network.MaxPacketSize)
                {
                    Logger.Error("Celestial League", $"Packet too large: {packetData.Length} bytes (max: {Shared.Constants.Network.MaxPacketSize})");
                    SendErrors++;
                    return false;
                }

                await _sendSemaphore.WaitAsync();
                try
                {
                    var lengthBytes = BitConverter.GetBytes(packetData.Length);
                    await stream.WriteAsync(lengthBytes, 0, 4);
                    await stream.WriteAsync(packetData, 0, packetData.Length);
                    await stream.FlushAsync();
                }
                finally
                {
                    _sendSemaphore.Release();
                }

                PacketsSent++;
                LastActivity = DateTime.UtcNow;

                Logger.Debug("Celestial League", $"Sent {typeof(T).Name} ({packetData.Length} bytes)");
                return true;
            }
            catch (IOException ex)
            {
                Logger.Error("Celestial League", $"IO error sending {typeof(T).Name}: {ex.Message}");
                SendErrors++;
                return false;
            }
            catch (ObjectDisposedException)
            {
                Logger.Error("Celestial League", $"Stream disposed while sending {typeof(T).Name}");
                SendErrors++;
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error("Celestial League", $"Unexpected error sending {typeof(T).Name}: {ex.Message}");
                SendErrors++;
                return false;
            }
        }


        public async Task<BasePacket> ReceivePacketAsync(NetworkStream stream, byte[] buffer, CancellationToken cancellationToken)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(NetworkClient));

            if (stream == null || !stream.CanRead)
            {
                Logger.Warn("Celestial League", "Cannot receive packet: Stream is null or not readable");
                return null;
            }

            try
            {
                var lengthBytes = new byte[4];
                var bytesRead = await ReadExactAsync(stream, lengthBytes, 4, cancellationToken);

                if (bytesRead != 4)
                {
                    Logger.Warn("Celestial League", "Failed to read packet length header");
                    ReceiveErrors++;
                    return null;
                }

                var packetLength = BitConverter.ToInt32(lengthBytes, 0);

                if (packetLength <= 0 || packetLength > Shared.Constants.Network.MaxPacketSize)
                {
                    Logger.Error("Celestial League", $"Invalid packet length: {packetLength}");
                    ReceiveErrors++;
                    return null;
                }

                if (buffer.Length < packetLength)
                {
                    Logger.Error("Celestial League", $"Buffer too small for packet: {buffer.Length} < {packetLength}");
                    ReceiveErrors++;
                    return null;
                }

                bytesRead = await ReadExactAsync(stream, buffer, packetLength, cancellationToken);

                if (bytesRead != packetLength)
                {
                    Logger.Warn("Celestial League", $"Incomplete packet received: {bytesRead}/{packetLength} bytes");
                    ReceiveErrors++;
                    return null;
                }

                var packetData = new byte[packetLength];
                Array.Copy(buffer, 0, packetData, 0, packetLength);

                var packet = Serialization.DeserializePacket(packetData, packetLength);

                if (packet == null)
                {
                    Logger.Error("Celestial League", "Failed to deserialize received packet");
                    ReceiveErrors++;
                    return null;
                }

                PacketsReceived++;
                LastActivity = DateTime.UtcNow;

                Logger.Debug("Celestial League", $"Received {packet.Type} ({packetLength} bytes)");
                return packet;
            }
            catch (OperationCanceledException)
            {
                // expected when cancellation is requested
                throw;
            }
            catch (IOException ex)
            {
                Logger.Error("Celestial League", $"IO error receiving packet: {ex.Message}");
                ReceiveErrors++;
                return null;
            }
            catch (ObjectDisposedException)
            {
                Logger.Warn("Celestial League", "Stream disposed while receiving packet");
                ReceiveErrors++;
                return null;
            }
            catch (Exception ex)
            {
                Logger.Error("Celestial League", $"Unexpected error receiving packet: {ex.Message}");
                ReceiveErrors++;
                return null;
            }
        }

        private async Task<int> ReadExactAsync(NetworkStream stream, byte[] buffer, int count, CancellationToken cancellationToken)
        {
            int totalBytesRead = 0;

            while (totalBytesRead < count && !cancellationToken.IsCancellationRequested)
            {
                var bytesRead = await stream.ReadAsync(
                    buffer,
                    totalBytesRead,
                    count - totalBytesRead,
                    cancellationToken);

                if (bytesRead == 0)
                {
                    break;
                }

                totalBytesRead += bytesRead;
            }

            return totalBytesRead;
        }

        public ConnectionQuality CalculateConnectionQuality()
        {
            if (PacketsSent == 0 && PacketsReceived == 0)
                return ConnectionQuality.VeryPoor;

            var totalPackets = PacketsSent + PacketsReceived;
            var totalErrors = SendErrors + ReceiveErrors;
            var errorRate = (double)totalErrors / totalPackets;

            var timeSinceLastActivity = DateTime.UtcNow - LastActivity;

            if (timeSinceLastActivity > TimeSpan.FromSeconds(30))
                return ConnectionQuality.Poor;
            else if (errorRate > 0.1) // more than 10% error rate
                return ConnectionQuality.Poor;
            else if (errorRate > 0.05) // more than 5% error rate
                return ConnectionQuality.Fair;
            else if (errorRate > 0.01) // more than 1% error rate
                return ConnectionQuality.Good;
            else
                return ConnectionQuality.Excellent;
        }

        public void ResetStatistics()
        {
            PacketsSent = 0;
            PacketsReceived = 0;
            SendErrors = 0;
            ReceiveErrors = 0;
            LastActivity = DateTime.UtcNow;

            Logger.Debug("Celestial League", "Network statistics reset");
        }

        public NetworkStatistics GetStatistics()
        {
            return new NetworkStatistics
            {
                PacketsSent = PacketsSent,
                PacketsReceived = PacketsReceived,
                SendErrors = SendErrors,
                ReceiveErrors = ReceiveErrors,
                LastActivity = LastActivity,
                ConnectionQuality = CalculateConnectionQuality()
            };
        }

        public void Dispose()
        {
            if (_disposed) return;

            _sendSemaphore?.Dispose();
            _disposed = true;
        }
    }

    public class NetworkStatistics
    {
        public int PacketsSent { get; set; }
        public int PacketsReceived { get; set; }
        public int SendErrors { get; set; }
        public int ReceiveErrors { get; set; }
        public DateTime LastActivity { get; set; }
        public ConnectionQuality ConnectionQuality { get; set; }
    }
}

