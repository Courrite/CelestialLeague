using System;
using Celeste;
using Celeste.Mod;

namespace CelestialLeague.Client.UI.Core
{
    public static class FontLoader
    {
        public static void LoadFonts()
        {
            var fontPaths = Fonts.paths;
            foreach (string key in fontPaths.Keys)
            {
                if (Fonts.Get(key) != null)
                {
                    Logger.Info("Celestial League", $"Font {key} is already loaded. Skipping.");
                    continue;
                }

                try
                {
                    Fonts.Load(key);
                    Logger.Info("Celestial League", $"Loaded {key} font.");
                }
                catch (Exception ex)
                {
                    Logger.Error("Celestial League", $"Error loading {key} font: {ex.Message}");
                }
            }

            Fonts.Reload();
        }
    }
}