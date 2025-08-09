using Celeste;
using Celeste.Mod;
using CelestialLeague.Client.Core;
using CelestialLeague.Client.Player;
using CelestialLeague.Client.Services;
using CelestialLeague.Client.UI.Core;
using FMOD.Studio;
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

            On.Monocle.Scene.Begin += static (orig, self) =>
            {
                orig(self);

                if (self is Level || self is Overworld)
                {
                    if (InterfaceManager.Instance == null)
                    {
                        self.Add(new InterfaceManager());
                    }
                    else
                    {
                        self.Add(InterfaceManager.Instance);
                    }
                }
            };

            Logger.Log(LogLevel.Info, "CelestialLeague", "Celestial League loaded");
        }

        public override void Unload()
        {
            _ = Task.Run(async () => await DisconnectAsync("Mod unloading"));

            Logger.Log(LogLevel.Info, "CelestialLeague", "CelestialLeague mod unloaded");
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
                    GameClient = new GameClient();
                }

                return success;
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "CelestialLeague", $"Connection error: {ex.Message}");
                GameClient?.Dispose();
                GameClient = new GameClient();
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