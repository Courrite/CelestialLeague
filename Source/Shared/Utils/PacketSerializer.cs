using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using CelestialLeague.Shared.Packets;
using CelestialLeague.Shared.Utils;

namespace CelestialLeague.Shared.Utils
{
    public static class PacketSerializer
    {
        private static readonly Dictionary<string, Type> _packetTypes = new();
        private static readonly object _lock = new();
        private static bool _initialized = false;

        private static void EnsureInitialized()
        {
            if (_initialized) return;

            lock (_lock)
            {
                if (_initialized) return;

                var packetTypes = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(assembly =>
                    {
                        try
                        {
                            return assembly.GetTypes();
                        }
                        catch (ReflectionTypeLoadException)
                        {
                            return Array.Empty<Type>();
                        }
                    })
                    .Where(t => t.IsSubclassOf(typeof(BasePacket)) && !t.IsAbstract && t.IsClass);

                foreach (var type in packetTypes)
                {
                    _packetTypes[type.Name] = type;
                }

                _initialized = true;
            }
        }

        public static BasePacket? Deserialize(byte[] buffer, int length)
        {
            try
            {
                EnsureInitialized();

                var span = new ReadOnlySpan<byte>(buffer, 0, length);
                var json = System.Text.Encoding.UTF8.GetString(span);

                using var jsonDoc = JsonDocument.Parse(json);
                if (!jsonDoc.RootElement.TryGetProperty("type", out var typeElement))
                {
                    return null;
                }

                var packetTypeName = typeElement.GetString();

                foreach (var (typeName, type) in _packetTypes)
                {
                    try
                    {
                        var tempInstance = Activator.CreateInstance(type) as BasePacket;
                        if (tempInstance?.Type.ToString() == packetTypeName)
                        {
                            var method = typeof(JsonSerializer).GetMethod(nameof(JsonSerializer.FromJson))
                                ?.MakeGenericMethod(type);

                            if (method != null)
                            {
                                var result = method.Invoke(null, new object[] { json });
                                if (result != null)
                                {
                                    return (BasePacket)result;
                                }
                            }
                        }
                    }
                    catch
                    {
                        continue;
                    }
                }

                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static byte[] Serialize<T>(T packet) where T : BasePacket
        {
            return JsonSerializer.ToBytes(packet);
        }
    }
}
