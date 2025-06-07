using CelestialLeague.Shared.Enums;

namespace CelestialLeague.Shared.Packets
{
    public abstract class BasePacket
    {
        public abstract PacketType Type { get; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public int Version { get; set; } = 1;
        public uint? CorrelationId { get; set; }

        private static uint _nextCorrelationId = 1;
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
