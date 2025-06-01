namespace CelestialLeague.Shared.Packets
{
    public abstract class Packet
    {
        public abstract PacketType Type { get; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public int Version { get; set; } = 1;
        public string? CorrelationId { get; set; }

        public virtual bool IsValid()
        {
            return !string.IsNullOrEmpty(Id) &&
            Timestamp != default &&
            Timestamp <= DateTime.UtcNow.AddMinutes(5); // swim clock skew
        }

        
    }
}