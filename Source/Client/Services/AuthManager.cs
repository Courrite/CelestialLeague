using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CelestialLeague.Client.Core;
using CelestialLeague.Client.Networking;
using CelestialLeague.Shared.Enum;
using CelestialLeague.Shared.Packets;
using CelestialLeague.Shared.Models;
using CelestialLeague.Shared.Utils;
using Celeste.Mod;

namespace CelestialLeague.Client.Services
{
    public class AuthManager : IDisposable
    {
        private static AuthManager _instance;
        private static readonly object _lock = new object();

        private readonly GameClient _gameClient;
        private ConnectionManager _connectionManager => _gameClient.ConnectionManager;
        private bool _disposed;

        public static AuthManager Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                        throw new InvalidOperationException("AuthManager not initialized. Call Initialize() first.");
                    return _instance;
                }
            }
        }

        private AuthManager(GameClient gameClient)
        {
            _gameClient = gameClient ?? throw new ArgumentNullException(nameof(gameClient));
        }

        public static void Initialize(GameClient gameClient)
        {
            lock (_lock)
            {
                _instance?.Dispose();
                _instance = new AuthManager(gameClient);
            }
        }

        public async Task<AuthResult> LoginAsync(string username, string password)
        {
            ArgumentNullException.ThrowIfNull(username, nameof(username));
            ArgumentNullException.ThrowIfNull(password, nameof(password));

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                return AuthResult.InvalidCredentials;
            }

            if (!_connectionManager.IsConnected)
            {
                return AuthResult.UnknownError;
            }

            var packet = new LoginRequestPacket(username, password, true);

            try
            {
                var testJson = JsonSerializer.ToJson(packet);
                Logger.Log(LogLevel.Debug, "CelestialLeague", $"Login request JSON: {testJson}");
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "CelestialLeague", $"Login serialization test failed: {ex.Message}");
            }

            try
            {
                var response = await _connectionManager.SendRequestAsync<LoginRequestPacket, LoginResponsePacket>(
                    packet,
                    TimeSpan.FromSeconds(30)
                ).ConfigureAwait(false);

                if (response == null)
                {
                    Logger.Log(LogLevel.Warn, "CelestialLeague", "Login request timed out or failed");
                    return AuthResult.Timeout;
                }

                Logger.Log(LogLevel.Info, "CelestialLeague", $"Login response received: Success={response.Success}");

                var result = response.Success
                    ? AuthResult.CreateSuccess(response.Player, response.SessionToken)
                    : AuthResult.CreateError(response.ErrorMessage ?? "Login failed");

                return result;
            }
            catch (InvalidOperationException ex)
            {
                Logger.Log(LogLevel.Error, "CelestialLeague", $"Login request error: {ex.Message}");
                return AuthResult.UnknownError;
            }
            catch (OperationCanceledException)
            {
                Logger.Log(LogLevel.Warn, "CelestialLeague", "Login request was cancelled");
                return AuthResult.Timeout;
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "CelestialLeague", $"Unexpected login error: {ex.Message}");
                return AuthResult.UnknownError;
            }
        }

        public async Task<AuthResult> RegisterAsync(string username, string password)
        {
            ArgumentNullException.ThrowIfNull(username, nameof(username));
            ArgumentNullException.ThrowIfNull(password, nameof(password));

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                return AuthResult.InvalidCredentials;
            }

            if (!_connectionManager.IsConnected)
            {
                return AuthResult.UnknownError;
            }

            var packet = new RegisterRequestPacket(username, password);

            // debug logging
            try
            {
                var testJson = JsonSerializer.ToJson(packet);
                Logger.Log(LogLevel.Debug, "CelestialLeague", $"Register request JSON: {testJson}");
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "CelestialLeague", $"Register serialization test failed: {ex.Message}");
            }

            try
            {
                var response = await _connectionManager.SendRequestAsync<RegisterRequestPacket, RegisterResponsePacket>(
                    packet,
                    TimeSpan.FromSeconds(30)
                ).ConfigureAwait(false);

                if (response == null)
                {
                    Logger.Log(LogLevel.Warn, "CelestialLeague", "Register request timed out or failed");
                    return AuthResult.Timeout;
                }

                Logger.Log(LogLevel.Info, "CelestialLeague", $"Register response received: Success={response.Success}");

                var result = response.Success
                    ? AuthResult.CreateSuccess(response.Player, response.SessionToken)
                    : AuthResult.CreateError(response.ErrorMessage ?? "Registration failed");

                return result;
            }
            catch (InvalidOperationException ex)
            {
                Logger.Log(LogLevel.Error, "CelestialLeague", $"Register request error: {ex.Message}");
                return AuthResult.UnknownError;
            }
            catch (OperationCanceledException)
            {
                Logger.Log(LogLevel.Warn, "CelestialLeague", "Register request was cancelled");
                return AuthResult.Timeout;
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "CelestialLeague", $"Unexpected register error: {ex.Message}");
                return AuthResult.UnknownError;
            }
        }

        public async Task<bool> LogoutAsync()
        {
            if (!_connectionManager.IsConnected)
                return false;

            try
            {
                var packet = new LogoutRequestPacket();
                var success = await _connectionManager.SendPacketAsync<LogoutRequestPacket>(packet).ConfigureAwait(false);

                Logger.Log(LogLevel.Info, "CelestialLeague", $"Logout request sent: {success}");
                return success;
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "CelestialLeague", $"Logout error: {ex.Message}");
                return false;
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
        }
    }

    public class AuthResult
    {
        public bool Success { get; private set; }
        public PlayerInfo PlayerInfo { get; private set; }
        public string SessionToken { get; private set; }
        public string ErrorMessage { get; private set; }

        private AuthResult(bool success, PlayerInfo playerInfo = null, string sessionToken = null, string errorMessage = null)
        {
            Success = success;
            PlayerInfo = playerInfo;
            SessionToken = sessionToken;
            ErrorMessage = errorMessage;
        }

        public static AuthResult CreateSuccess(PlayerInfo playerInfo, string sessionToken)
            => new(true, playerInfo, sessionToken);

        public static AuthResult CreateError(string errorMessage)
            => new(false, errorMessage: errorMessage);

        public static readonly AuthResult InvalidCredentials = new(false, errorMessage: "Invalid credentials");
        public static readonly AuthResult UnknownError = new(false, errorMessage: "Unknown error occurred");
        public static readonly AuthResult Timeout = new(false, errorMessage: "Request timed out");
    }
}

