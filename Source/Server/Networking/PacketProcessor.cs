using System.Collections.Concurrent;
using System.Reflection;
using CelestialLeague.Server.Core;
using CelestialLeague.Server.Networking;
using CelestialLeague.Server.Networking.PacketHandlers;
using CelestialLeague.Server.Utils;
using CelestialLeague.Shared.Enums;
using CelestialLeague.Shared.Packets;

namespace CelestialLeague.Server.Utils
{
    public class PacketProcessor
    {
        private readonly GameServer _gameServer;
        private Logger _logger => _gameServer.Logger;
        private readonly ConcurrentDictionary<PacketType, HandlerMethod> _handlers = new();
        private readonly ConcurrentDictionary<string, DateTime> _lastPacketTime = new();
        private readonly ConcurrentDictionary<string, int> _packetCounts = new();

        public PacketProcessor(GameServer gameServer, BaseHandler[] handlers)
        {
            _gameServer = gameServer ?? throw new ArgumentNullException(nameof(gameServer));
            
            ArgumentNullException.ThrowIfNull(handlers, nameof(handlers));
            RegisterHandlers(handlers);
            
            _logger.Info($"PacketProcessor initialized with {_handlers.Count} packet handlers");
        }

        public async Task ProcessAsync(ClientConnection connection, BasePacket packet)
        {
            if (connection == null || packet == null)
            {
                _logger.Warning("ProcessAsync called with null connection or packet");
                return;
            }

            var connectionId = connection.ConnectionID;
            var packetType = packet.Type;

            try
            {
                if (!CheckRateLimit(connectionId, packetType))
                {
                    _logger.Warning($"Rate limit exceeded for {connectionId} - {packetType}");
                    await connection.DisconnectAsync("Rate limit exceeded").ConfigureAwait(false);
                    return;
                }

                if (!_handlers.TryGetValue(packetType, out var handlerMethod))
                {
                    _logger.Warning($"No handler found for packet type: {packetType} from {connectionId}");
                    
                    if (packet.CorrelationId.HasValue)
                    {
                        var errorResponse = new ErrorPacket(ResponseErrorCode.InvalidPacket, $"No handler for packet type: {packetType}")
                        {
                            CorrelationId = packet.CorrelationId
                        };
                        await connection.SendPacketAsync(errorResponse).ConfigureAwait(false);
                    }
                    return;
                }

                if (handlerMethod.RequiresAuthentication && !connection.IsAuthenticated)
                {
                    _logger.Warning($"Unauthenticated client {connectionId} tried to send {packetType}");
                    
                    var authErrorResponse = new ErrorPacket(ResponseErrorCode.NotAuthenticated, "Authentication required")
                    {
                        CorrelationId = packet.CorrelationId,
                    };
                    await connection.SendPacketAsync(authErrorResponse).ConfigureAwait(false);
                    return;
                }

                _logger.Debug($"Processing {packetType} from {connectionId}");
                
                var response = await InvokeHandlerAsync(handlerMethod, packet, connectionId).ConfigureAwait(false);
                
                if (response != null)
                {
                    if (packet.CorrelationId.HasValue)
                    {
                        response.CorrelationId = packet.CorrelationId;
                    }
                    
                    await connection.SendPacketAsync(response).ConfigureAwait(false);
                    _logger.Debug($"Sent {response.Type} response to {connectionId}");
                }

                _logger.Debug($"Successfully processed {packetType} from {connectionId}");
            }
            catch (Exception ex)
            {
                _logger.Error($"Error processing {packetType} from {connectionId}: {ex.Message}");
                _logger.Debug($"Stack trace: {ex.StackTrace}");

                try
                {
                    if (packet.CorrelationId.HasValue)
                    {
                        var errorResponse = new ErrorPacket(ResponseErrorCode.InternalError, "An error occurred processing your request")
                        {
                            CorrelationId = packet.CorrelationId
                        };
                        await connection.SendPacketAsync(errorResponse).ConfigureAwait(false);
                    }
                }
                catch (Exception sendEx)
                {
                    _logger.Error($"Failed to send error response to {connectionId}: {sendEx.Message}");
                }
            }
        }

        private void RegisterHandlers(BaseHandler[] handlers)
        {
            foreach (var handler in handlers)
            {
                RegisterHandlerMethods(handler);
            }
        }

        private void RegisterHandlerMethods(BaseHandler handler)
        {
            var handlerType = handler.GetType();
            var methods = handlerType.GetMethods(BindingFlags.Public | BindingFlags.Instance);

            foreach (var method in methods)
            {
                var attribute = method.GetCustomAttribute<PacketHandlerAttribute>();
                if (attribute == null) continue;

                try
                {
                    ValidateHandlerMethod(method);
                    
                    var handlerMethod = new HandlerMethod
                    {
                        Handler = handler,
                        Method = method,
                        PacketType = attribute.PacketType,
                        RequiresAuthentication = attribute.RequiresAuthentication
                    };

                    if (_handlers.TryAdd(attribute.PacketType, handlerMethod))
                    {
                        _logger.Info($"Registered handler: {handlerType.Name}.{method.Name} for {attribute.PacketType}");
                    }
                    else
                    {
                        _logger.Warning($"Duplicate handler for {attribute.PacketType}: {handlerType.Name}.{method.Name}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error($"Failed to register handler {handlerType.Name}.{method.Name}: {ex.Message}");
                }
            }
        }

        private static void ValidateHandlerMethod(MethodInfo method)
        {
            if (!typeof(Task<BasePacket>).IsAssignableFrom(method.ReturnType) && 
                !typeof(Task).IsAssignableFrom(method.ReturnType))
            {
                throw new InvalidOperationException($"Handler method {method.Name} must return Task<BasePacket> or Task");
            }

            var parameters = method.GetParameters();
            if (parameters.Length != 2)
            {
                throw new InvalidOperationException($"Handler method {method.Name} must have exactly 2 parameters: (BasePacket packet, string connectionId)");
            }

            if (!typeof(BasePacket).IsAssignableFrom(parameters[0].ParameterType))
            {
                throw new InvalidOperationException($"First parameter of {method.Name} must be a BasePacket or derived type");
            }

            if (parameters[1].ParameterType != typeof(string))
            {
                throw new InvalidOperationException($"Second parameter of {method.Name} must be string (connectionId)");
            }
        }

        private async Task<BasePacket?> InvokeHandlerAsync(HandlerMethod handlerMethod, BasePacket packet, string connectionId)
        {
            try
            {
                var result = handlerMethod.Method.Invoke(handlerMethod.Handler, new object[] { packet, connectionId });
                
                if (result is Task<BasePacket> taskWithResult)
                {
                    return await taskWithResult.ConfigureAwait(false);
                }
                else if (result is Task task)
                {
                    await task.ConfigureAwait(false);
                    return null;
                }
                else
                {
                    _logger.Error($"Handler method returned unexpected type: {result?.GetType()}");
                    return null;
                }
            }
            catch (TargetInvocationException ex)
            {
                throw ex.InnerException ?? ex;
            }
        }

        private bool CheckRateLimit(string connectionId, PacketType packetType)
        {
            var now = DateTime.UtcNow;
            var key = $"{connectionId}:{packetType}";

            var (maxPackets, timeWindow) = GetRateLimits(packetType);

            if (now.Second % 10 == 0)
            {
                CleanupRateLimitData(now);
            }

            if (!_lastPacketTime.TryGetValue(key, out var lastTime) || 
                (now - lastTime).TotalSeconds >= timeWindow)
            {
                _lastPacketTime[key] = now;
                _packetCounts[key] = 1;
                return true;
            }

            var currentCount = _packetCounts.AddOrUpdate(key, 1, (k, v) => v + 1);
            
            return currentCount <= maxPackets;
        }

        private static (int maxPackets, int timeWindowSeconds) GetRateLimits(PacketType packetType)
        {
            return packetType switch
            {
                // auth packets
                PacketType.LoginRequest => (5, 60),
                PacketType.RegisterRequest => (3, 300),
                
                // game packets
                PacketType.PlayerPosition => (NetworkConstants.MaxPositionUpdatesPerSecond, 1),
                
                // chat packets
                PacketType.ChatMessage => (ChatConstants.MaxMessagesPerMinute, 60),
                
                // general packets
                PacketType.Heartbeat => (2, 30),
                PacketType.Ping => (10, 60),
                
                // unknown packets
                _ => (30, 60)
            };
        }

        private void CleanupRateLimitData(DateTime now)
        {
            var keysToRemove = new List<string>();

            foreach (var kvp in _lastPacketTime)
            {
                if ((now - kvp.Value).TotalMinutes > 10)
                {
                    keysToRemove.Add(kvp.Key);
                }
            }

            foreach (var key in keysToRemove)
            {
                _lastPacketTime.TryRemove(key, out _);
                _packetCounts.TryRemove(key, out _);
            }
        }

        public Dictionary<string, object> GetStatistics()
        {
            return new Dictionary<string, object>
            {
                ["RegisteredHandlers"] = _handlers.Count,
                ["ActiveRateLimitEntries"] = _lastPacketTime.Count,
                ["HandlerTypes"] = _handlers.Values.Select(h => h.PacketType.ToString()).ToArray()
            };
        }

        public bool HasHandler(PacketType packetType)
        {
            return _handlers.ContainsKey(packetType);
        }

        public IEnumerable<PacketType> GetSupportedPacketTypes()
        {
            return _handlers.Keys.ToArray();
        }
    }

    internal class HandlerMethod
    {
        public required BaseHandler Handler { get; set; }
        public required MethodInfo Method { get; set; }
        public required PacketType PacketType { get; set; }
        public required bool RequiresAuthentication { get; set; }
    }
}
