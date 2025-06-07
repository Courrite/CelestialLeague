using CelestialLeague.Shared.Packets;

namespace CelestialLeague.Shared.Utils
{
    public static class PacketSerializer
    {
        public static byte[] Serialize<T>(T packet) where T : BasePacket
        {
            Validate(packet);
            
            var data = JsonSerializer.ToBytes(packet);
            return ShouldCompress(data) ? Compression.Compress(data) : data;
        }

        public static BasePacket? Deserialize(byte[] data)
        {
            if (!IsValidSize(data)) return null;
            
            var packet = JsonSerializer.FromBytes<BasePacket>(data);
            return packet?.IsValid() == true ? packet : null;
        }

        private static void Validate<T>(T packet) where T : BasePacket
        {
            if (packet?.IsValid() != true)
                throw new ArgumentException("Invalid packet", nameof(packet));
        }

        private static bool ShouldCompress(byte[] data) => 
            data.Length > NetworkConstants.CompressionThreshold;

        private static bool IsValidSize(byte[] data) => 
            data.Length <= NetworkConstants.MaxPacketSize;
    }
}