using CelestialLeague.Shared.Enums;

namespace CelestialLeague.Shared.Utils
{
    public class StateHelpers
    {
        public static byte SetFlag(byte currentFlags, PlayerStateFlags flag)
        {
            return (byte)(currentFlags | (byte)flag);
        }

        public static byte ClearFlag(byte currentFlags, PlayerStateFlags flag)
        {
            return (byte)(currentFlags & ~(byte)flag);
        }

        public static bool HasFlag(byte currentFlags, PlayerStateFlags flag)
        {
            return (currentFlags & (byte)flag) != 0;
        }

        public static byte ToggleFlag(byte currentFlags, PlayerStateFlags flag)
        {
            return (byte)(currentFlags ^ (byte)flag);
        }

        public static byte SetFlags(params PlayerStateFlags[] flags)
        {
            byte result = 0;
            foreach (var flag in flags)
            {
                result |= (byte)flag;
            }
            return result;
        }

        public static List<PlayerStateFlags> GetActiveFlags(byte stateFlags)
        {
            var activeFlags = new List<PlayerStateFlags>();
            foreach (PlayerStateFlags flag in Enum.GetValues<PlayerStateFlags>())
            {
                if (flag != PlayerStateFlags.None && HasFlag(stateFlags, flag))
                {
                    activeFlags.Add((flag));
                }
            }
            return activeFlags;
        }
    }
}