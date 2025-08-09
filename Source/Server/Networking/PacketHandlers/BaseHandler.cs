<<<<<<< HEAD
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
=======
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
>>>>>>> 48bc47b13401bb7e2dfc20bc611c767893bc8e52
}