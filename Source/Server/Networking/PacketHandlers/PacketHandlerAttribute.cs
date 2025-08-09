using CelestialLeague.Shared.Enums;

namespace CelestialLeague.Server.Networking.PacketHandlers
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class PacketHandlerAttribute : Attribute
    {
        public PacketType PacketType { get; }
        public bool RequiresAuthentication { get; private set; } = true;

        public PacketHandlerAttribute(PacketType packetType, bool requiresAuthentication = true)
        {
            PacketType = packetType;
            RequiresAuthentication = requiresAuthentication;
        }
    }
}