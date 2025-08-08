using CelestialLeague.Shared.Enums;

namespace CelestialLeague.Shared.Packets
{
    public abstract class BasePacket
    {
        public abstract PacketType Type { get; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public int Version { get; set; } = 1;
        public uint? CorrelationId { get; set; }

        protected BasePacket()
        {
            Timestamp = DateTime.UtcNow;
            Version = 1;
            CorrelationId = 0;
        }

        protected BasePacket(bool generateCorrelationId)
        {
            Timestamp = DateTime.UtcNow;
            Version = 1;
            CorrelationId = generateCorrelationId ? GenerateCorrelationId() : 0;
        }

        protected static uint GenerateCorrelationId()
        {
            var timestamp = (uint)
            (DateTimeOffset.UtcNow.ToUnixTimeSeconds() & 0xFFFFFF);
            var random = (uint)Random.Shared.Next(0, 256);
            return (timestamp << 8) | random;
        }

        public virtual bool IsValid()
        {
            return Timestamp != default &&
                   Timestamp <= DateTime.UtcNow.AddMinutes(5); // allow clock skew
        }
    }
}
