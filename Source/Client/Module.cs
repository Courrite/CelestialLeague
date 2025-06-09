using Celeste;
using Celeste.Mod;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Threading.Tasks;
using CelestialLeague.Client.Core;
using CelestialLeague.Client.Networking;
using CelestialLeague.Shared.Packets;
using CelestialLeague.Shared.Enums;

namespace CelestialLeague.Client
{
    public class CelestialLeagueModule : EverestModule
    {
        public static CelestialLeagueModule Instance { get; private set; }
        
        public override Type SettingsType => typeof(CelestialLeagueSettings);
        public static CelestialLeagueSettings Settings => (CelestialLeagueSettings)Instance._Settings;

        private GameClient _gameClient;
        private bool _isConnecting;
        private bool _isConnected;

        public bool IsConnected => _gameClient?.IsConnected == true;
        public bool IsConnecting => _isConnecting;
        public GameClient GameClient => _gameClient;

        public event EventHandler<PacketReceivedEventArgs> OnPacketReceived;
        public event EventHandler<DisconnectedEventArgs> OnDisconnected;

        public CelestialLeagueModule()
        {
            Instance = this;
        }

        public override void Load()
        {
            Logger.Log(LogLevel.Info, "CelestialLeague", "Loading Celestial League...");
            
            try
            {
                _gameClient = new GameClient();
                _gameClient.OnDisconnected += OnGameClientDisconnected;
                
                Logger.Log(LogLevel.Info, "CelestialLeague", "Celestial League loaded successfully");
                
                Task.Run(async () => await AutoConnectAsync());
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "CelestialLeague", $"Failed to load Celestial League: {ex.Message}");
            }
        }

        private async Task AutoConnectAsync()
        {
            try
            {
                await Task.Delay(2000);
                
                Logger.Log(LogLevel.Info, "CelestialLeague", "Starting auto-connection...");
                
                var host = Settings?.ServerHost ?? "127.0.0.1";
                var port = Settings?.ServerPort ?? 7777;
                var autoConnect = Settings?.AutoConnect ?? true;
                
                if (!autoConnect)
                {
                    Logger.Log(LogLevel.Info, "CelestialLeague", "Auto-connect disabled in settings");
                    return;
                }
                
                var success = await ConnectAsync(host, port);
                
                if (success)
                {
                    Logger.Log(LogLevel.Info, "CelestialLeague", "Auto-connection successful");
                }
                else
                {
                    Logger.Log(LogLevel.Warn, "CelestialLeague", "Auto-connection failed - will retry later");
                    
                    _ = Task.Run(async () => await RetryConnectionAsync(host, port));
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "CelestialLeague", $"Auto-connection error: {ex.Message}");
            }
        }

        private async Task RetryConnectionAsync(string host, int port)
        {
            const int maxRetries = 3;
            const int retryDelayMs = 10000; // 10 seconds
            
            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    Logger.Log(LogLevel.Info, "CelestialLeague", $"Connection retry {attempt}/{maxRetries}...");
                    
                    await Task.Delay(retryDelayMs);
                    
                    if (IsConnected)
                    {
                        Logger.Log(LogLevel.Info, "CelestialLeague", "Already connected, stopping retries");
                        return;
                    }
                    
                    var success = await ConnectAsync(host, port);
                    
                    if (success)
                    {
                        Logger.Log(LogLevel.Info, "CelestialLeague", $"Connection successful on retry {attempt}");
                        return;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log(LogLevel.Error, "CelestialLeague", $"Retry {attempt} failed: {ex.Message}");
                }
            }
            
            Logger.Log(LogLevel.Warn, "CelestialLeague", $"All {maxRetries} connection attempts failed");
        }

        public override void Unload()
        {
            Logger.Log(LogLevel.Info, "CelestialLeague", "Unloading Celestial League...");
            
            try
            {
                if (_gameClient != null)
                {
                    if (_gameClient.IsConnected)
                    {
                        _gameClient.DisconnectAsync("Module unloading").Wait(TimeSpan.FromSeconds(2));
                    }
                    
                    _gameClient.Dispose();
                    _gameClient = null;
                }
                
                Logger.Log(LogLevel.Info, "CelestialLeague", "Celestial League unloaded successfully");
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "CelestialLeague", $"Error during unload: {ex.Message}");
            }
        }

        public async Task<bool> ConnectAsync(string host = "127.0.0.1", int port = 7777)
        {
            if (_isConnecting || IsConnected)
                return IsConnected;

            try
            {
                _isConnecting = true;
                Logger.Log(LogLevel.Info, "CelestialLeague", $"Connecting to {host}:{port}...");
                
                var timeout = TimeSpan.FromSeconds(Settings?.ConnectionTimeout ?? 10);
                var success = await _gameClient!.ConnectAsync(host, port, timeout);
                
                if (success)
                {
                    _isConnected = true;
                    Logger.Log(LogLevel.Info, "CelestialLeague", "Connected to server successfully");
                }
                else
                {
                    Logger.Log(LogLevel.Warn, "CelestialLeague", "Failed to connect to server");
                }
                
                return success;
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "CelestialLeague", $"Connection error: {ex.Message}");
                return false;
            }
            finally
            {
                _isConnecting = false;
            }
        }

        public async Task DisconnectAsync(string reason = "User disconnected")
        {
            if (!IsConnected)
                return;

            try
            {
                Logger.Log(LogLevel.Info, "CelestialLeague", $"Disconnecting: {reason}");
                await _gameClient!.DisconnectAsync(reason);
                _isConnected = false;
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "CelestialLeague", $"Disconnect error: {ex.Message}");
            }
        }

        private void OnGameClientDisconnected(object sender, DisconnectedEventArgs e)
        {
            _isConnected = false;
            Logger.Log(LogLevel.Info, "CelestialLeague", $"Disconnected from server: {e.Message}");
            OnDisconnected?.Invoke(this, e);
            
            if (Settings?.AutoReconnect == true && !e.Message.Contains("Module unloading") && !e.Message.Contains("User disconnected"))
            {
                Logger.Log(LogLevel.Info, "CelestialLeague", "Attempting auto-reconnect...");
                var host = Settings?.ServerHost ?? "127.0.0.1";
                var port = Settings?.ServerPort ?? 7777;
                _ = Task.Run(async () => await RetryConnectionAsync(host, port));
            }
        }
    }
}
