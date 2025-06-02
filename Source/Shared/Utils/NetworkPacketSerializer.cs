using CelestialLeague.Shared.Packets;
using CelestialLeague.Shared.Enums;

namespace CelestialLeague.Shared.Utils
{
    public static class NetworkPacketSerializer
    {
        public static byte[] Create<T>(T packet) where T : BasePacket
        {
            var data = PacketSerializer.Serialize(packet);
            ValidateSize(data);
            
            return CombineHeaderAndData(
                CreateHeader(packet.Type, data.Length), 
                data
            );
        }

        public static (PacketType Type, byte[] Data)? Parse(byte[] networkData)
        {
            if (!HasValidHeader(networkData)) return null;

            try
            {
                var (type, length) = ReadHeader(networkData);
                var data = ExtractData(networkData, length);
                
                return IsValidPacket(data, length) ? (type, data) : null;
            }
            catch
            {
                return null;
            }
        }

        private static byte[] CreateHeader(PacketType type, int length)
        {
            var header = new byte[NetworkConstants.PacketHeaderSize];
            BitConverter.GetBytes((int)type).CopyTo(header, 0);
            BitConverter.GetBytes(length).CopyTo(header, 4);
            return header;
        }

        private static (PacketType type, int length) ReadHeader(byte[] data) => 
            ((PacketType)BitConverter.ToInt32(data, 0), BitConverter.ToInt32(data, 4));

        private static byte[] ExtractData(byte[] networkData, int length)
        {
            var data = new byte[length];
            Array.Copy(networkData, NetworkConstants.PacketHeaderSize, data, 0, length);
            return data;
        }

        private static byte[] CombineHeaderAndData(byte[] header, byte[] data)
        {
            var result = new byte[header.Length + data.Length];
            Array.Copy(header, 0, result, 0, header.Length);
            Array.Copy(data, 0, result, header.Length, data.Length);
            return result;
        }

        private static bool HasValidHeader(byte[] data) => 
            data.Length >= NetworkConstants.PacketHeaderSize;

        private static bool IsValidPacket(byte[] data, int expectedLength) => 
            data.Length == expectedLength && expectedLength <= NetworkConstants.MaxPacketSize;

        private static void ValidateSize(byte[] data)
        {
            if (data.Length > NetworkConstants.MaxPacketSize)
                throw new ArgumentException($"Packet too large: {data.Length} bytes");
        }
    }
}