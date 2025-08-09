<<<<<<< HEAD
using System;
using System.Threading.Tasks;
using CelestialLeague.Shared.Models;
using CelestialLeague.Shared.Enums;
using CelestialLeague.Client.Services;

namespace CelestialLeague.Client.Player
{
    public class LocalPlayer
    {
        private static LocalPlayer _instance;
        private static readonly object _lock = new object();

        public static LocalPlayer Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        _instance ??= new LocalPlayer();
                    }
                }
                return _instance;
            }
        }

        private PlayerInfo _playerInfo;
        private bool _isAuthenticated;
        private string _sessionToken;

        public PlayerInfo PlayerInfo => _playerInfo;
        public bool IsAuthenticated => _isAuthenticated;
        public string SessionToken => _sessionToken;
        public int PlayerId => _playerInfo?.Id ?? 0;
        public string Username => _playerInfo?.Username;
        public PlayerStatus Status => _playerInfo?.PlayerStatus ?? PlayerStatus.Offline;
        public UserRole Role => _playerInfo?.UserRole ?? UserRole.None;

        public event Action<PlayerInfo> OnPlayerInfoLoaded;
        public event Action OnLoggedIn;
        public event Action OnLoggedOut;
        public event Action<string> OnAuthenticationFailed;

        private LocalPlayer()
        {
            _isAuthenticated = false;
        }

        public async Task<bool> LoginAsync(string username, string password)
        {
            try
            {
                var authManager = AuthManager.Instance;
                var result = await authManager.LoginAsync(username, password);

                if (result.Success)
                {
                    SetAuthenticatedState(result.PlayerInfo, result.SessionToken);
                    return true;
                }
                else
                {
                    OnAuthenticationFailed?.Invoke(result.ErrorMessage ?? "Login failed");
                    return false;
                }
            }
            catch (Exception ex)
            {
                OnAuthenticationFailed?.Invoke($"Login error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> RegisterAsync(string username, string password)
        {
            try
            {
                var authManager = AuthManager.Instance;
                var result = await authManager.RegisterAsync(username, password);

                if (result.Success)
                {
                    SetAuthenticatedState(result.PlayerInfo, result.SessionToken);
                    return true;
                }
                else
                {
                    OnAuthenticationFailed?.Invoke(result.ErrorMessage ?? "Registration failed");
                    return false;
                }
            }
            catch (Exception ex)
            {
                OnAuthenticationFailed?.Invoke($"Registration error: {ex.Message}");
                return false;
            }
        }

        public void Logout()
        {
            try
            {
                if (_isAuthenticated)
                {
                    var authManager = AuthManager.Instance;
                    _ = Task.Run(async () => await authManager.LogoutAsync());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during server logout: {ex.Message}");
            }
            finally
            {
                ClearAuthenticatedState();
            }
        }

        public void SetAuthenticatedState(PlayerInfo playerInfo, string sessionToken)
        {
            _playerInfo = playerInfo;
            _playerInfo.PlayerStatus = PlayerStatus.Online;
            _playerInfo.LastSeen = DateTime.UtcNow;

            _sessionToken = sessionToken;
            _isAuthenticated = true;

            OnPlayerInfoLoaded?.Invoke(_playerInfo);
            OnLoggedIn?.Invoke();
        }

        public void ClearAuthenticatedState()
        {
            if (_playerInfo != null)
            {
                _playerInfo.PlayerStatus = PlayerStatus.Offline;
                _playerInfo.LastSeen = DateTime.UtcNow;
            }

            _playerInfo = null;
            _sessionToken = null;
            _isAuthenticated = false;

            OnLoggedOut?.Invoke();
        }

        public void UpdatePlayerInfo(PlayerInfo updatedPlayerInfo)
        {
            if (_isAuthenticated && _playerInfo != null && _playerInfo.Id == updatedPlayerInfo.Id)
            {
                _playerInfo = updatedPlayerInfo;
                OnPlayerInfoLoaded?.Invoke(_playerInfo);
            }
        }

        public bool ValidateSession()
        {
            return _isAuthenticated && 
                   !string.IsNullOrEmpty(_sessionToken) && 
                   _playerInfo != null;
        }

        public void HandleConnectionLost()
        {
            if (_playerInfo != null)
            {
                _playerInfo.PlayerStatus = PlayerStatus.Offline;
            }
            // dont clear authentication state, might reconnect
        }

        public void HandleReconnected()
        {
            if (_playerInfo != null)
            {
                _playerInfo.PlayerStatus = PlayerStatus.Online;
                _playerInfo.LastSeen = DateTime.UtcNow;
            }
        }

        public void UpdateStatus(PlayerStatus status)
        {
            if (_playerInfo != null)
            {
                _playerInfo.PlayerStatus = status;
                _playerInfo.LastSeen = DateTime.UtcNow;
            }
        }
    }
}
=======
using System;
using System.Threading.Tasks;
using CelestialLeague.Shared.Models;
using CelestialLeague.Shared.Enums;
using CelestialLeague.Client.Services;

namespace CelestialLeague.Client.Player
{
    public class LocalPlayer
    {
        private static LocalPlayer _instance;
        private static readonly object _lock = new object();

        public static LocalPlayer Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        _instance ??= new LocalPlayer();
                    }
                }
                return _instance;
            }
        }

        private PlayerInfo _playerInfo;
        private bool _isAuthenticated;
        private string _sessionToken;

        public PlayerInfo PlayerInfo => _playerInfo;
        public bool IsAuthenticated => _isAuthenticated;
        public string SessionToken => _sessionToken;
        public int PlayerId => _playerInfo?.Id ?? 0;
        public string Username => _playerInfo?.Username;
        public PlayerStatus Status => _playerInfo?.PlayerStatus ?? PlayerStatus.Offline;
        public UserRole Role => _playerInfo?.UserRole ?? UserRole.None;

        public event Action<PlayerInfo> OnPlayerInfoLoaded;
        public event Action OnLoggedIn;
        public event Action OnLoggedOut;
        public event Action<string> OnAuthenticationFailed;

        private LocalPlayer()
        {
            _isAuthenticated = false;
        }

        public async Task<bool> LoginAsync(string username, string password)
        {
            try
            {
                var authManager = AuthManager.Instance;
                var result = await authManager.LoginAsync(username, password);

                if (result.Success)
                {
                    SetAuthenticatedState(result.PlayerInfo, result.SessionToken);
                    return true;
                }
                else
                {
                    OnAuthenticationFailed?.Invoke(result.ErrorMessage ?? "Login failed");
                    return false;
                }
            }
            catch (Exception ex)
            {
                OnAuthenticationFailed?.Invoke($"Login error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> RegisterAsync(string username, string password)
        {
            try
            {
                var authManager = AuthManager.Instance;
                var result = await authManager.RegisterAsync(username, password);

                if (result.Success)
                {
                    SetAuthenticatedState(result.PlayerInfo, result.SessionToken);
                    return true;
                }
                else
                {
                    OnAuthenticationFailed?.Invoke(result.ErrorMessage ?? "Registration failed");
                    return false;
                }
            }
            catch (Exception ex)
            {
                OnAuthenticationFailed?.Invoke($"Registration error: {ex.Message}");
                return false;
            }
        }

        public void Logout()
        {
            try
            {
                if (_isAuthenticated)
                {
                    var authManager = AuthManager.Instance;
                    _ = Task.Run(async () => await authManager.LogoutAsync());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during server logout: {ex.Message}");
            }
            finally
            {
                ClearAuthenticatedState();
            }
        }

        public void SetAuthenticatedState(PlayerInfo playerInfo, string sessionToken)
        {
            _playerInfo = playerInfo;
            _playerInfo.PlayerStatus = PlayerStatus.Online;
            _playerInfo.LastSeen = DateTime.UtcNow;

            _sessionToken = sessionToken;
            _isAuthenticated = true;

            OnPlayerInfoLoaded?.Invoke(_playerInfo);
            OnLoggedIn?.Invoke();
        }

        public void ClearAuthenticatedState()
        {
            if (_playerInfo != null)
            {
                _playerInfo.PlayerStatus = PlayerStatus.Offline;
                _playerInfo.LastSeen = DateTime.UtcNow;
            }

            _playerInfo = null;
            _sessionToken = null;
            _isAuthenticated = false;

            OnLoggedOut?.Invoke();
        }

        public void UpdatePlayerInfo(PlayerInfo updatedPlayerInfo)
        {
            if (_isAuthenticated && _playerInfo != null && _playerInfo.Id == updatedPlayerInfo.Id)
            {
                _playerInfo = updatedPlayerInfo;
                OnPlayerInfoLoaded?.Invoke(_playerInfo);
            }
        }

        public bool ValidateSession()
        {
            return _isAuthenticated && 
                   !string.IsNullOrEmpty(_sessionToken) && 
                   _playerInfo != null;
        }

        public void HandleConnectionLost()
        {
            if (_playerInfo != null)
            {
                _playerInfo.PlayerStatus = PlayerStatus.Offline;
            }
            // dont clear authentication state, might reconnect
        }

        public void HandleReconnected()
        {
            if (_playerInfo != null)
            {
                _playerInfo.PlayerStatus = PlayerStatus.Online;
                _playerInfo.LastSeen = DateTime.UtcNow;
            }
        }

        public void UpdateStatus(PlayerStatus status)
        {
            if (_playerInfo != null)
            {
                _playerInfo.PlayerStatus = status;
                _playerInfo.LastSeen = DateTime.UtcNow;
            }
        }
    }
}
>>>>>>> 48bc47b13401bb7e2dfc20bc611c767893bc8e52
