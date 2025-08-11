using Celeste;
using Celeste.Mod;
using CelestialLeague.Client.Core;
using CelestialLeague.Client.Player;
using CelestialLeague.Client.Services;
using CelestialLeague.Client.UI.Core;
using CelestialLeague.Client.Resources;
using FMOD.Studio;
using Monocle;
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

            On.Monocle.Scene.Begin += OnSceneBegin;

            Logger.Log(LogLevel.Info, "CelestialLeague", "Celestial League loaded");
        }

        public override void Unload()
        {
            _ = Task.Run(async () => await DisconnectAsync("Mod unloading"));

            On.Monocle.Scene.Begin -= OnSceneBegin;

            Logger.Log(LogLevel.Info, "CelestialLeague", "CelestialLeague mod unloaded");
        }

        private static void OnSceneBegin(On.Monocle.Scene.orig_Begin orig, Scene self)
        {
            orig(self);

            if (self is Level || self is Overworld)
            {
                if (InterfaceManager.Instance == null)
                {
                    self.Add(new InterfaceManager());
                    Logger.Log(LogLevel.Verbose, "CelestialLeague", $"Created InterfaceManager for {self.GetType().Name}");
                }
                else
                {
                    if (InterfaceManager.Instance.Scene != self)
                    {
                        InterfaceManager.Instance.RemoveSelf();

                        self.Add(InterfaceManager.Instance);
                        Logger.Log(LogLevel.Verbose, "CelestialLeague", $"Moved InterfaceManager to {self.GetType().Name}");
                    }
                }
            }
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

            menu.Add(new TextMenu.SubHeader("UI Controls"));

            var uiManager = InterfaceManager.Instance;
            if (uiManager != null)
            {
                var toggleUIButton = new TextMenu.Button(uiManager.IsVisible ? "Hide UI" : "Show UI");
                toggleUIButton.Pressed(() =>
                {
                    uiManager?.Toggle();
                    toggleUIButton.Label = uiManager?.IsVisible == true ? "Hide UI" : "Show UI";
                });
                menu.Add(toggleUIButton);

                menu.Add(new TextMenu.Button("Reset UI").Pressed(() =>
                {
                    try
                    {
                        uiManager?.ClearChildren();
                        Logger.Log(LogLevel.Info, "CelestialLeague", "UI reset");
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(LogLevel.Error, "CelestialLeague", $"Error resetting UI: {ex.Message}");
                    }
                }));
            }
            else
            {
                menu.Add(new TextMenu.Button("UI: Not Active"));
            }

            menu.Add(new TextMenu.Button("Reload Fonts").Pressed(() =>
            {
                try
                {
                    Logger.Log(LogLevel.Info, "CelestialLeague", "Font cache cleared");
                }
                catch (Exception ex)
                {
                    Logger.Log(LogLevel.Error, "CelestialLeague", $"Error clearing font cache: {ex.Message}");
                }
            }));

            if (Settings.ShowDebugInfo)
            {
                menu.Add(new TextMenu.SubHeader("Debug Info"));
                menu.Add(new TextMenu.Button($"UI Manager: {(uiManager != null ? "Active" : "None")}"));

                if (uiManager != null)
                {
                    menu.Add(new TextMenu.Button($"UI Visible: {uiManager.IsVisible}"));
                    menu.Add(new TextMenu.Button($"UI Scene: {uiManager.Scene?.GetType().Name ?? "None"}"));
                }
            }
        }
    }
}