using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
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
        public static byte[] SerializePacket<T>(T packet) where T : Packet => 
            PacketSerializer.Serialize(packet);
        public static T? DeserializePacket<T>(byte[] data) where T : Packet => 
            PacketSerializer.Deserialize<T>(data);
        
        // network packets
        public static byte[] SerializeNetworkPacket<T>(T packet) where T : Packet => 
            NetworkPacket.Create(packet);
        public static (PacketType Type, byte[] Data)? DeserializeNetworkPacket(byte[] data) => 
            NetworkPacket.Parse(data);
    }
}