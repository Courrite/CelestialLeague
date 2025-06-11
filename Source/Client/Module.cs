using Celeste;
using Celeste.Mod;
using Celeste.Mod.UI;
using CelestialLeague.Client.Core;
using CelestialLeague.Client.Player;
using CelestialLeague.Client.Services;
using CelestialLeague.Shared.Models;
using FMOD.Studio;
using MonoMod.Utils;
using System;
using System.Threading.Tasks;

namespace CelestialLeague.Client
{
    public class CelestialLeagueModule : EverestModule
    {
        public static CelestialLeagueModule Instance { get; private set; }
        public override Type SettingsType => typeof(CelestialLeagueSettings);
        public static CelestialLeagueSettings Settings => (CelestialLeagueSettings)Instance._Settings;

        public GameClient GameClient { get; private set; }

        public override void Load()
        {
            Instance = this;
            GameClient = new GameClient();
            AuthManager.Initialize(GameClient);

            if (Settings.AutoConnect)
            {
                _ = Task.Run(async () => await ConnectAsync(Settings.ServerHost, Settings.ServerPort));
            }

            Logger.Log(LogLevel.Info, "CelestialLeague", "CelestialLeague mod loaded");
        }
        public override void Unload()
        {
            _ = Task.Run(async () => await DisconnectAsync("Mod unloading"));

            Logger.Log(LogLevel.Info, "CelestialLeague", "CelestialLeague mod unloaded");
        }

        private void AddTestingButtons(TextMenu menu, bool inGame)
        {
            menu.Add(new TextMenu.SubHeader("Testing Functions"));

            menu.Add(new TextMenu.Button("Connect to Server").Pressed(() =>
            {
                Logger.Log(LogLevel.Info, "CelestialLeague", "Connect button pressed");
                _ = Task.Run(async () =>
                {
                    try
                    {
                        var success = await ConnectAsync(Settings.ServerHost, Settings.ServerPort);
                        Logger.Log(LogLevel.Info, "CelestialLeague", $"Connect result: {success}");
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(LogLevel.Error, "CelestialLeague", $"Connect error: {ex.Message}");
                    }
                });
            }));

            menu.Add(new TextMenu.Button("Disconnect from Server").Pressed(() =>
            {
                Logger.Log(LogLevel.Info, "CelestialLeague", "Disconnect button pressed");
                _ = Task.Run(async () => await DisconnectAsync("Manual disconnect"));
            }));

            menu.Add(new TextMenu.Button("Test Login").Pressed(() =>
            {
                Logger.Log(LogLevel.Info, "CelestialLeague", "Test login button pressed");
                _ = Task.Run(async () =>
                {
                    try
                    {
                        var result = await AuthManager.Instance.LoginAsync(Settings.TestUsername, Settings.TestPassword);
                        Logger.Log(LogLevel.Info, "CelestialLeague", $"Login result: Success={result.Success}");
                        if (result.Success && result.PlayerInfo != null)
                        {
                            Logger.Log(LogLevel.Info, "CelestialLeague", $"Player Info: ID={result.PlayerInfo.Id}, Username={result.PlayerInfo.Username}");
                        }
                        else if (!result.Success)
                        {
                            Logger.Log(LogLevel.Warn, "CelestialLeague", $"Login failed: {result.ErrorMessage}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(LogLevel.Error, "CelestialLeague", $"Login error: {ex.Message}");
                    }
                });
            }));

            menu.Add(new TextMenu.Button("Test Register").Pressed(() =>
            {
                Logger.Log(LogLevel.Info, "CelestialLeague", "Test register button pressed");
                _ = Task.Run(async () =>
                {
                    try
                    {
                        var result = await AuthManager.Instance.RegisterAsync(
                            Settings.TestUsername,
                            Settings.TestPassword
                        );
                        Logger.Log(LogLevel.Info, "CelestialLeague", $"Register result: Success={result.Success}");
                        if (result.Success && result.PlayerInfo != null)
                        {
                            Logger.Log(LogLevel.Info, "CelestialLeague", $"Player Info: ID={result.PlayerInfo.Id}, Username={result.PlayerInfo.Username}");
                        }
                        else if (!result.Success)
                        {
                            Logger.Log(LogLevel.Warn, "CelestialLeague", $"Register failed: {result.ErrorMessage}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(LogLevel.Error, "CelestialLeague", $"Register error: {ex.Message}");
                    }
                });
            }));

            menu.Add(new TextMenu.Button("Check Connection Status").Pressed(() =>
            {
                try
                {
                    var isConnected = GameClient?.IsConnected ?? false;
                    var authStatus = LocalPlayer.Instance.IsAuthenticated ? "Authenticated" : "Not Authenticated";
                    var currentPlayer = LocalPlayer.Instance.Username ?? "None";

                    Logger.Log(LogLevel.Info, "CelestialLeague", $"Connection Status: {isConnected}");
                    Logger.Log(LogLevel.Info, "CelestialLeague", $"Auth Status: {authStatus}");
                    Logger.Log(LogLevel.Info, "CelestialLeague", $"Current Player: {currentPlayer}");
                }
                catch (Exception ex)
                {
                    Logger.Log(LogLevel.Error, "CelestialLeague", $"Check status error: {ex.Message}");
                }
            }));

            menu.Add(new TextMenu.Button("Test Logout").Pressed(() =>
            {
                Logger.Log(LogLevel.Info, "CelestialLeague", "Test logout button pressed");
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await AuthManager.Instance.LogoutAsync();
                        Logger.Log(LogLevel.Info, "CelestialLeague", "Logout completed");
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(LogLevel.Error, "CelestialLeague", $"Logout error: {ex.Message}");
                    }
                });
            }));
        }

        public async Task<bool> ConnectAsync(string host, int port)
        {
            try
            {
                if (GameClient?.IsConnected == true)
                {
                    Logger.Log(LogLevel.Info, "CelestialLeague", "Already connected to server");
                    return true;
                }

                Logger.Log(LogLevel.Info, "CelestialLeague", $"Connecting to {host}:{port}...");

                var success = await GameClient.ConnectAsync(host, port, TimeSpan.FromSeconds(Settings.ConnectionTimeout));

                if (success)
                {
                    Logger.Log(LogLevel.Info, "CelestialLeague", "Successfully connected to server");
                }
                else
                {
                    Logger.Log(LogLevel.Warn, "CelestialLeague", "Failed to connect to server");
                    GameClient?.Dispose();
                    GameClient = null;
                }

                return success;
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "CelestialLeague", $"Connection error: {ex.Message}");
                GameClient?.Dispose();
                GameClient = null;
                return false;
            }
        }

        public async Task DisconnectAsync(string reason = "User requested")
        {
            try
            {
                if (GameClient?.IsConnected == true)
                {
                    Logger.Log(LogLevel.Info, "CelestialLeague", $"Disconnecting from server: {reason}");
                    await GameClient.DisconnectAsync();
                }

                GameClient?.Dispose();
                GameClient = null;

                if (LocalPlayer.Instance.IsAuthenticated)
                {
                    await AuthManager.Instance.LogoutAsync();
                }

                Logger.Log(LogLevel.Info, "CelestialLeague", "Disconnected from server");
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "CelestialLeague", $"Disconnect error: {ex.Message}");
            }
        }

        public override void CreateModMenuSection(TextMenu menu, bool inGame, EventInstance snapshot)
        {
            base.CreateModMenuSection(menu, inGame, snapshot);

            Logger.Log(LogLevel.Info, "CelestialLeague", "Adding testing buttons to mod menu");
            AddTestingButtons(menu, inGame);

            if (GameClient?.IsConnected == true)
            {
                menu.Add(new TextMenu.Button("Disconnect").Pressed(() =>
                {
                    _ = Task.Run(async () => await DisconnectAsync("Menu disconnect"));
                }));
            }
            else
            {
                menu.Add(new TextMenu.Button("Quick Connect").Pressed(() =>
                {
                    _ = Task.Run(async () => await ConnectAsync(Settings.ServerHost, Settings.ServerPort));
                }));
            }
        }
    }
}