using System.IO.Compression;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace CelestialLeague.Shared.Utils
{
    public class Serialization
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        // json serialization
        public static string ToJson<T>(T obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        public static T? FromJson<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }

        public static byte[] ToJsonBytes<T>(T obj)
        {
            return Encoding.UTF8.GetBytes(ToJson(obj));
        }

        public static T? FromJsonBytes<T>(byte[] bytes)
        {
            string json = Encoding.UTF8.GetString(bytes);
            return FromJson<T>(json);
        }

        // binary serialization
        public static byte[] ToBinary<T>(T obj)
        {
            var json = ToJson(obj);
            return Encoding.UTF8.GetBytes(json);
        }

        public static T? FromBinary<T>(byte[] bytes)
        {
            var json = Encoding.UTF8.GetString(bytes);
            return FromJson<T>(json);
        }

        // compressed serialization
        public static byte[] ToCompressed<T>(T obj)
        {
            var jsonBytes = ToJsonBytes(obj);
            using var output = new MemoryStream();
            using (var gzip = new GZipStream(output, CompressionMode.Compress))
            {
                gzip.Write(jsonBytes, 0, jsonBytes.Length);
            }
            return output.ToArray();
        }

        public static T? FromCompressed<T>(byte[] compressedData)
        {
            using var input = new MemoryStream(compressedData);
            using var gzip = new GZipStream(input, CompressionMode.Decompress);
            using var output = new MemoryStream();
            gzip.CopyTo(output);
            return FromJsonBytes<T>(output.ToArray());
        }

        // packet serialization
        public static byte[] SerializePacket<T>(T packet)
        {
            return ToJsonBytes(packet);
        }

        public static T? DeserializePacket<T>(byte[] packetData)
        {
            return FromJsonBytes<T>(packetData);
        }

        // safe serialization
        public static bool TrySerialize<T>(T obj, out byte[] result)
        {
            try
            {
                result = ToJsonBytes(obj);
                return true;
            }
            catch
            {
                result = Array.Empty<byte>();
                return false;
            }
        }

        public static bool TryDeserialize<T>(byte[] data, out T? result)
        {
            try
            {
                result = FromJsonBytes<T>(data);
                return true;
            }
            catch
            {
                result = default;
                return false;
            }
        }

        // size helpers
        public static int GetSerializedSize<T>(T obj)
        {
            return ToJsonBytes(obj).Length;
        }

        // validation
        public static bool IsValidJson(string json)
        {
            try
            {
                JsonDocument.Parse(json);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}