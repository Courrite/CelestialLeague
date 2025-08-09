using System.Globalization;
using CelestialLeague.Server.Models;
using CelestialLeague.Server.Utils;
using CelestialLeague.Shared.Enums;
using CelestialLeague.Shared.Utils;
using Microsoft.EntityFrameworkCore;
using CelestialLeague.Server.Database.Context;
using System.Text.RegularExpressions;
using CelestialLeague.Server.Core;
using System.Threading.Tasks;
using CelestialLeague.Shared.Models;

namespace CelestialLeague.Server.Services
{
    public class AuthenticationService
    {
        private readonly GameServer _gameServer;
        private GameDbContext _context => _gameServer.GameDbContext;
        private Logger _logger => _gameServer.Logger;
        private SessionManager _sessionManager => _gameServer.SessionManager;

        public AuthenticationService(GameServer gameServer)
        {
            _gameServer = gameServer ?? throw new ArgumentNullException(nameof(gameServer));
        }

        public async Task<AuthResult> RegisterAsync(string username, string password)
        {
            var validationResult = ValidateRegistrationInput(username, password);
            if (validationResult != AuthResult.Success)
                return validationResult;

            if (await IsUsernameTakenAsync(username).ConfigureAwait(false))
                return AuthResult.UsernameTaken;

            try
            {
                var salt = SecurityHelpers.GenerateSalt();
                var hash = SecurityHelpers.HashPassword(password, salt);

                var player = new Player(username, hash, salt, DateTime.UtcNow);

                _context.Players.Add(player);
                await _context.SaveChangesAsync().ConfigureAwait(false);

                _logger.Info($"New player registered: {username}");
                return AuthResult.Success;
            }
            catch (Exception ex)
            {
                _logger.Error($"Registration failed for {username}: {ex.Message}");
                return AuthResult.DatabaseError;
            }
        }

        public async Task<(AuthResult Result, PlayerInfo PlayerInfo, string? SessionToken)> LoginAsync(string username, string password)
        {
            try
            {
                var player = await _context.Players
                    .FirstOrDefaultAsync(p => p.Username == username)
                    .ConfigureAwait(false);

                if (player == null)
                {
                    _logger.Warning($"Login attempt with non-existent username: {username}");
                    return (AuthResult.InvalidCredentials, null!, null);
                }

                if (!SecurityHelpers.VerifyPassword(password, player.PasswordHash, player.PasswordSalt))
                {
                    _logger.Warning($"Invalid password attempt for user: {username}");
                    return (AuthResult.InvalidCredentials, null!, null);
                }

                player.LastSeen = DateTime.UtcNow;
                await _context.SaveChangesAsync().ConfigureAwait(false);

                var sessionToken = await _sessionManager.CreateSessionAsync(player.Id).ConfigureAwait(false);

                _logger.Info($"User logged in successfully: {username}");
                return (AuthResult.Success, new PlayerInfo(player.Id, player.Username, player.CreatedAt), sessionToken);
            }
            catch (Exception ex)
            {
                _logger.Error($"Login failed for {username}: {ex.Message}");
                return (AuthResult.DatabaseError, null!, null);
            }
        }

        private async Task<bool> IsUsernameTakenAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return false;

            try
            {
                return await _context.Players
                    .AnyAsync(p => p.Username == username)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.Error($"Error checking username availability for {username}: {ex}");
                return true;
            }
        }

        public async Task<Player?> ValidationSessionAsync(string sessionToken)
        {
            try
            {
                if (!await _sessionManager.IsSessionValidAsync(sessionToken).ConfigureAwait(false))
                    return null;

                var playerId = await _sessionManager.GetPlayerIdAsync(sessionToken).ConfigureAwait(false);
                if (playerId == null)
                    return null;

                var player = await _context.Players
                    .FindAsync(playerId.Value)
                    .ConfigureAwait(false);

                return player;
            }
            catch (Exception ex)
            {
                _logger.Error($"Session validation failed for token {sessionToken}: {ex.Message}");
                return null;
            }
        }

        public async Task LogoutAsync(string sessionToken)
        {
            try
            {
                await _sessionManager.InvalidateSessionAsync(sessionToken).ConfigureAwait(false);
                _logger.Info($"User logged out with session: {sessionToken}");
            }
            catch (Exception ex)
            {
                _logger.Error($"Logout failed for session {sessionToken}: {ex.Message}");
            }
        }

        public async Task<bool> ChangePasswordAsync(string sessionToken, string oldPassword, string newPassword)
        {
            try
            {
                var player = await ValidationSessionAsync(sessionToken).ConfigureAwait(false);
                if (player == null)
                {
                    _logger.Warning("Password change attempt with invalid session");
                    return false;
                }

                if (!SecurityHelpers.VerifyPassword(oldPassword, player.PasswordHash, player.PasswordSalt))
                {
                    _logger.Warning($"Password change failed - incorrect old password for user: {player.Username}");
                    return false;
                }

                var newSalt = SecurityHelpers.GenerateSalt();
                var newHash = SecurityHelpers.HashPassword(newPassword, newSalt);

                player.PasswordHash = newHash;
                player.PasswordSalt = newSalt;

                await _context.SaveChangesAsync().ConfigureAwait(false);

                _logger.Info($"Password changed successfully for user: {player.Username}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error($"Password change failed for session {sessionToken}: {ex.Message}");
                return false;
            }
        }

        private static bool IsValidUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return false;

            if (username.Length < GameConstants.MinUsernameLength ||
                username.Length > GameConstants.MaxUsernameLength)
                return false;

            var allowedCharactersRegex = new Regex(@"^[a-zA-Z0-9_]+$");
            if (!allowedCharactersRegex.IsMatch(username))
                return false;

            if (username.StartsWith('_') || char.IsDigit(username[0]))
                return false;

            return true;
        }

        private static AuthResult ValidateRegistrationInput(string username, string password)
        {
            if (!IsValidUsername(username))
                return AuthResult.InvalidUsername;

            return AuthResult.Success;
        }

        private static string GetErrorMessage(AuthResult result)
        {
            return result switch
            {
                AuthResult.Success => "Operation completed successfully",
                AuthResult.InvalidCredentials => "Invalid username or password",
                AuthResult.UsernameTaken => "Username is already taken",
                AuthResult.InvalidUsername => "Username is invalid or contains forbidden characters",
                AuthResult.TooManyAttempts => "Too many failed login attempts",
                AuthResult.SessionExpired => "Session has expired",
                AuthResult.DatabaseError => "Database connection failed",
                AuthResult.AccountLocked => "Account is temporarily locked",
                _ => "Unknown error occurred"
            };
        }

        public static string GetUserFriendlyErrorMessage(AuthResult result)
        {
            return GetErrorMessage(result);
        }
    }
}