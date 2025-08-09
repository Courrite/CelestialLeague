using CelestialLeague.Server.Core;
using CelestialLeague.Server.Utils;

namespace CelestialLeague.Server.Networking.PacketHandlers
{
    public abstract class BaseHandler
    {
        protected GameServer GameServer { get; }
        protected Logger Logger => GameServer.Logger;

        protected BaseHandler(GameServer gameServer)
        {
            GameServer = gameServer ?? throw new ArgumentNullException(nameof(gameServer));
        }
    }
}