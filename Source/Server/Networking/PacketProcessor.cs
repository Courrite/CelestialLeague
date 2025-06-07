using CelestialLeague.Server.Core;
using CelestialLeague.Server.Models;
using CelestialLeague.Server.Utils;
using CelestialLeague.Shared.Enums;
using CelestialLeague.Shared.Packets;

namespace CelestialLeague.Server.Networking
{
    public class PacketProcessor
    {
        private GameServer _gameServer { get; set; }
        private Logger _logger => _gameServer.Logger;
        private Dictionary<PacketType, Func<ClientConnection, object, Task>> _packetHandlers;

        public PacketProcessor(GameServer gameServer)
        {
            _gameServer = gameServer;
            RegisterHandlers();
        }

        public void RegisterHandlers()
        {
            _packetHandlers = new()
            {
                [PacketType.Heartbeat] = async (conn, packet) => await HandleHeartbeatPacket(conn, (HeartbeatPacket)packet).ConfigureAwait(false),
            };
        }

        public async Task ProcessAsync(ClientConnection connection, BasePacket packet)
        {
            try
            {
                if (!packet.IsValid())
                {
                    _logger.Warning($"invalid packet received from {connection.ConnectionID}: {packet.Type}");
                    await SendErrorPacket(connection, ResponseErrorCode.InvalidPacket, "Invalid packet received").ConfigureAwait(false);
                    return;
                }

                connection.UpdateActivity();

                _logger.Debug($"processing {packet.Type} from {connection.ConnectionID} ({packet.CorrelationId})");

                if (_packetHandlers.TryGetValue(packet.Type, out var handler))
                {
                    await handler(connection, packet).ConfigureAwait(false);
                }
            }
            catch
            {
                await SendErrorPacket(connection, ResponseErrorCode.InternalError, "Internal server error").ConfigureAwait(false);
            }
        }

        private async Task SendErrorPacket(ClientConnection connection, ResponseErrorCode errorCode, string message, string? details = null)
        {
            try
            {
                var errorPacket = new ErrorPacket(errorCode, message, details);

                await connection.SendPacketAsync(errorPacket).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.Error($"failed to send error packet to {connection.ConnectionID}: {ex.Message}");
            }
        }

        public async Task SendAckPacket(ClientConnection connection, uint? correlationId)
        {
            try
            {
                var ackPacket = new AcknowledgmentPacket(correlationId);
                await connection.SendPacketAsync(ackPacket).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.Error($"failed to send ack packet to {connection.ConnectionID}: {ex.Message}");
            }
        }

        public async Task HandleHeartbeatPacket(ClientConnection connection, HeartbeatPacket packet)
        {
            try
            {
                await SendAckPacket(connection, packet.CorrelationId).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.Error($"failed to send ack packet to {connection.ConnectionID}: {ex.Message}");
            }
        }
    }
}