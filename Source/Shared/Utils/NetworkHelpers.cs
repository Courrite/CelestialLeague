namespace CelestialLeague.Shared.Utils
{
    public class NetworkHelpers
    {
        // connection methods
        public static async Task<int> GetPingAsync(string hostname)
        {
            await Task.Delay(1000);
            return 0;
        }

        // validation methods
        public static bool IsValidPacketSized(byte[] data)
        {
            return data?.Length > 0 && data.Length <= NetworkConstants.MaxPacketSize;
        }

        public static bool IsValidMessageLength(string message)
        {
            return message?.Length > 0 && message.Length <= NetworkConstants.MaxMessageLength;
        }
    }
}