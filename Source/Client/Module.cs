using Celeste;
using Celeste.Mod;
using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace CelestialLeague.Client
{
    public class CelestialLeagueModule : EverestModule
    {
        // Singleton
        public static CelestialLeagueModule Instance { get; private set; }

        // Settings
        public override Type SettingsType => typeof(CelestialLeagueSettings);
        public static CelestialLeagueSettings Settings => (CelestialLeagueSettings)Instance._Settings;

        public CelestialLeagueModule()
        {
            Instance = this;
        }

        public override void Load()
        {
            Logger.Log(LogLevel.Info, "CelestialLeague", "Loading Celestial League...");

            // TODO: Initialize components
            // TODO: Setup hooks
        }

        public override void Unload()
        {
            Logger.Log(LogLevel.Info, "CelestialLeague", "Unloading Celestial League...");

            // TODO: Cleanup components
            // TODO: Remove hooks
        }

        // TODO: Add hook methods here
        // private void SomeHook(On.Celeste.Something.orig_Method orig, args...)

        public void Update()
        {
            // TODO: Update components
        }

        public void Render()
        {
            // TODO: Render components
        }
    }
}