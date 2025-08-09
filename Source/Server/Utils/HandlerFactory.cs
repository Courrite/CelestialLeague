<<<<<<< HEAD
using System.Reflection;
using CelestialLeague.Server.Core;
using CelestialLeague.Server.Networking.PacketHandlers;

namespace CelestialLeague.Server.Utils
{
    public static class HandlerFactory
    {
        public static BaseHandler[] CreateAllHandlers(GameServer gameServer)
        {
            var handlers = new List<BaseHandler>();
            
            var handlerTypes = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => t.IsSubclassOf(typeof(BaseHandler)) && !t.IsAbstract)
                .ToArray();

            foreach (var handlerType in handlerTypes)
            {
                try
                {
                    var handler = (BaseHandler)Activator.CreateInstance(handlerType, gameServer)!;
                    handlers.Add(handler);
                    
                    gameServer?.Logger.Info($"Auto-created handler: {handlerType.Name}");
                }
                catch (MissingMethodException ex)
                {
                    gameServer?.Logger.Error($"Failed to create handler {handlerType.Name}: {ex.Message}");
                }
                catch (ArgumentException ex)
                {
                    gameServer?.Logger.Error($"Failed to create handler {handlerType.Name}: {ex.Message}");
                }
                catch (TargetInvocationException ex)
                {
                    gameServer?.Logger.Error($"Failed to create handler {handlerType.Name}: {ex.Message}");
                }
            }

            return handlers.ToArray();
        }
    }
=======
using System.Reflection;
using CelestialLeague.Server.Core;
using CelestialLeague.Server.Networking.PacketHandlers;

namespace CelestialLeague.Server.Utils
{
    public static class HandlerFactory
    {
        public static BaseHandler[] CreateAllHandlers(GameServer gameServer)
        {
            var handlers = new List<BaseHandler>();
            
            var handlerTypes = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => t.IsSubclassOf(typeof(BaseHandler)) && !t.IsAbstract)
                .ToArray();

            foreach (var handlerType in handlerTypes)
            {
                try
                {
                    var handler = (BaseHandler)Activator.CreateInstance(handlerType, gameServer)!;
                    handlers.Add(handler);
                    
                    gameServer?.Logger.Info($"Auto-created handler: {handlerType.Name}");
                }
                catch (MissingMethodException ex)
                {
                    gameServer?.Logger.Error($"Failed to create handler {handlerType.Name}: {ex.Message}");
                }
                catch (ArgumentException ex)
                {
                    gameServer?.Logger.Error($"Failed to create handler {handlerType.Name}: {ex.Message}");
                }
                catch (TargetInvocationException ex)
                {
                    gameServer?.Logger.Error($"Failed to create handler {handlerType.Name}: {ex.Message}");
                }
            }

            return handlers.ToArray();
        }
    }
>>>>>>> 48bc47b13401bb7e2dfc20bc611c767893bc8e52
}