using CelestialLeague.Shared.Packets;
using CelestialLeague.Shared.Enums;

namespace CelestialLeague.Shared.Utils
{
    public static class Serialization
    {
        // json
        public static string ToJson<T>(T obj) => JsonSerializer.ToJson(obj);
        public static T? FromJson<T>(string json) => JsonSerializer.FromJson<T>(json);
        
        // compressed data
        public static byte[] ToCompressed<T>(T obj) => 
            Compression.Compress(JsonSerializer.ToBytes(obj));
        public static T? FromCompressed<T>(byte[] data) => 
            JsonSerializer.FromBytes<T>(Compression.Decompress(data));
        
        // packets
        public static byte[] SerializePacket<T>(T packet) where T : BasePacket => 
            PacketSerializer.Serialize(packet);
        public static BasePacket? DeserializePacket(byte[] data, int length) => 
            PacketSerializer.Deserialize(data, length);
    }
}