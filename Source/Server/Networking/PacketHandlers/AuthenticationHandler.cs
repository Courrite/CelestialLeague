using CelestialLeague.Server.Core;
using CelestialLeague.Server.Models;
using CelestialLeague.Shared.Enums;
using CelestialLeague.Shared.Models;
using CelestialLeague.Shared.Packets;

namespace CelestialLeague.Server.Networking.PacketHandlers
{
    public class AuthenticationHandler : BaseHandler
    {
        public AuthenticationHandler(GameServer server) : base(server)
        { }

        [PacketHandler(PacketType.LoginRequest, requiresAuthentication: false)]

        public async Task<BasePacket> HandleLoginAsync(LoginRequestPacket packet, string connectionId)
        {
            ArgumentNullException.ThrowIfNull(packet, nameof(packet));
            Logger.Info($"Login attempt from {connectionId}: {packet.Username}");

            try
            {

                var (result, playerInfo, sessionToken) = await GameServer.AuthenticationService.LoginAsync(packet.Username, packet.Password).ConfigureAwait(false);

                if (result == AuthResult.Success && sessionToken != null)
                {
                    var connection = GameServer.GetConnection(connectionId);
                    if (connection != null)
                    {
                        await GameServer.SetConnectionAsync(sessionToken, connectionId).ConfigureAwait(false);
                        var session = await GameServer.GetSessionByConnectionAsync(connectionId).ConfigureAwait(false);
                        if (session != null)
                        {
                            connection.SetSession(session);
                        }
                    }

                    var player = await GameServer.AuthenticationService.ValidationSessionAsync(sessionToken).ConfigureAwait(false);

                    Logger.Info($"Successful login for {packet.Username} on connection {connectionId}");

                    return new LoginResponsePacket
                    {
                        Success = true,
                        Message = "Login successful",
                        SessionToken = sessionToken,
                        Player = player != null ? new PlayerInfo(player.Id, player.Username, player.CreatedAt)
                        {
                            Id = player.Id,
                            Username = player.Username,
                            LastSeen = player.LastSeen
                        } : null
                    };
                }

                Logger.Warning($"Failed login attempt for {packet.Username} from {connectionId}: {result}");

                return new LoginResponsePacket
                {
                    Success = false,
                    ErrorCode = MapAuthResultToErrorCode(result),
                    Message = GetErrorMessage(result)
                };
            }
            catch (Exception ex)
            {
                Logger.Error($"Login error for connection {connectionId}: {ex.Message}");

                return new LoginResponsePacket
                {
                    Success = false,
                    ErrorCode = ResponseErrorCode.InternalError,
                    Message = "An unexpected error occurred"
                };
            }
        }

        [PacketHandler(PacketType.RegisterRequest, requiresAuthentication: false)]
        public async Task<BasePacket> HandleRegisterAsync(RegisterRequestPacket packet, string connectionId)
        {
            ArgumentNullException.ThrowIfNull(packet, nameof(packet));
            Logger.Info($"Registration attempt from {connectionId}: {packet.Username}");

            try
            {
                var result = await GameServer.AuthenticationService.RegisterAsync(packet.Username, packet.Password).ConfigureAwait(false);
                if (result == AuthResult.Success)
                {
                    Logger.Info($"Successful registration for {packet.Username} from {connectionId}");

                    var loginResult = await GameServer.AuthenticationService.LoginAsync(packet.Username, packet.Password).ConfigureAwait(false);
                    if (loginResult.Result == AuthResult.Success)
                    {
                        var playerInfo = loginResult.PlayerInfo;
                        var sessionToken = await GameServer.SessionManager.CreateSessionAsync(playerInfo.Id).ConfigureAwait(false);

                        Logger.Info($"Auto-login successful for newly registered user {packet.Username}");

                        return new RegisterResponsePacket(packet.CorrelationId, true)
                        {
                            SessionToken = sessionToken,
                            Player = loginResult.PlayerInfo,
                            Message = "Registration and login successful"
                        };
                    }
                    else
                    {
                        Logger.Warning($"Registration succeeded but auto-login failed for {packet.Username}.");
                        return new RegisterResponsePacket(packet.CorrelationId, true)
                        {
                            SessionToken = null,
                            Player = null,
                            Message = "Registration successful, but auto-login failed. Please login manually."
                        };
                    }
                }

                Logger.Warning($"Failed registration attempt for {packet.Username} from {connectionId}: {result}");
                return new RegisterResponsePacket(packet.CorrelationId, false)
                {
                    ErrorCode = MapAuthResultToErrorCode(result),
                    Message = GetErrorMessage(result)
                };
            }
            catch (Exception ex)
            {
                Logger.Error($"Registration error for connection {connectionId}: {ex.Message}");
                return new RegisterResponsePacket(packet.CorrelationId, false)
                {
                    ErrorCode = ResponseErrorCode.InternalError,
                    Message = "An unexpected error occurred"
                };
            }
        }


        [PacketHandler(PacketType.LogoutRequest, requiresAuthentication: true)]
        public async Task HandleLogoutAsync(LogoutRequestPacket packet, string connectionId)
        {
            Logger.Info($"Logout request from {connectionId}");

            try
            {
                var connection = GameServer.GetConnection(connectionId);
                if (connection?.Session != null)
                {
                    var sessionToken = connection.Session.SessionToken;

                    await GameServer.AuthenticationService.LogoutAsync(sessionToken).ConfigureAwait(false);
                    connection.ClearSession();

                    Logger.Info($"User logged out from connection {connectionId}");
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Logout error for connection {connectionId}: {ex.Message}");
            }
        }

        private static ResponseErrorCode MapAuthResultToErrorCode(AuthResult result)
        {
            return result switch
            {
                AuthResult.InvalidCredentials => ResponseErrorCode.InvalidCredentials,
                AuthResult.UsernameTaken => ResponseErrorCode.InvalidRequest,
                AuthResult.InvalidUsername => ResponseErrorCode.InvalidRequest,
                AuthResult.DatabaseError => ResponseErrorCode.ServerError,
                _ => ResponseErrorCode.InternalError
            };
        }

        private static string GetErrorMessage(AuthResult result)
        {
            return result switch
            {
                AuthResult.InvalidCredentials => "Invalid username or password",
                AuthResult.TooManyAttempts => "Too many login attempts. Please try again later",
                AuthResult.UsernameTaken => "Username is already taken",
                AuthResult.InvalidUsername => "Username is invalid",
                AuthResult.DatabaseError => "Service temporarily unavailable",
                _ => "Login failed"
            };
        }
    }
}