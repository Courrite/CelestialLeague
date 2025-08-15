using Celeste;
using Celeste.Mod;
using CelestialLeague.Client.Networking;
using CelestialLeague.Client.Services;
using CelestialLeague.Client.UI.Core;
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
        public InterfaceManager InterfaceManager { get; private set; }

        public bool IsConnecting { get; private set; }

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
        }

        public override void Unload()
        {
            _ = Task.Run(async () => await DisconnectAsync("Mod unloading"));

            On.Monocle.Scene.Begin -= OnSceneBegin;

            InterfaceManager?.RemoveSelf();
            InterfaceManager = null;
        }

        private static void OnSceneBegin(On.Monocle.Scene.orig_Begin orig, Scene self)
        {
            orig(self);

            if (self is Level || self is Overworld)
            {
                if (Instance.InterfaceManager == null || Instance.InterfaceManager.Scene != self)
                {
                    Instance.InterfaceManager?.RemoveSelf();
                    Instance.InterfaceManager = new InterfaceManager();
                    self.Add(Instance.InterfaceManager);
                }
            }
            else if (Instance.InterfaceManager != null)
            {
                Instance.InterfaceManager.RemoveSelf();
                Instance.InterfaceManager = null;
            }
        }

        public async Task<bool> ConnectAsync(string host, int port)
        {
            try
            {
                if (GameClient?.IsConnected == true || IsConnecting)
                {
                    Logger.Log(LogLevel.Info, "Celestial League", "Already connected or connecting to server");
                    return true;
                }

                IsConnecting = true;

                var success = await GameClient.ConnectAsync(host, port, TimeSpan.FromSeconds(Settings.ConnectionTimeout));

                if (!success)
                {
                    Logger.Log(LogLevel.Warn, "Celestial League", "Failed to connect to server");
                    GameClient?.Dispose();
                    GameClient = new GameClient();
                }

                return success;
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "Celestial League", $"Connection error: {ex.Message}");
                GameClient?.Dispose();
                GameClient = new GameClient();
                return false;
            }
            finally
            {
                IsConnecting = false;
            }
        }

        public async Task DisconnectAsync(string reason = "User requested")
        {
            try
            {
                if (GameClient?.IsConnected == true)
                {
                    await GameClient.DisconnectAsync();
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "Celestial League", $"Disconnect error: {ex.Message}");
            }
        }
    }
}