using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CelestialLeague.Shared.Utils
{
    public static class JsonSerializer
    {
        private static readonly JsonSerializerOptions Options = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };

        public static string ToJson<T>(T obj) => 
            System.Text.Json.JsonSerializer.Serialize(obj, Options);

        public static T? FromJson<T>(string json)
        {
            try { return System.Text.Json.JsonSerializer.Deserialize<T>(json, Options); }
            catch { return default; }
        }

        public static byte[] ToBytes<T>(T obj) => 
            Encoding.UTF8.GetBytes(ToJson(obj));

        public static T? FromBytes<T>(byte[] data)
        {
            try { return FromJson<T>(Encoding.UTF8.GetString(data)); }
            catch { return default; }
        }
    }

}