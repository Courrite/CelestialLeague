using System;
using Celeste;
using Celeste.Mod;
using Celeste.Mod.Core;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework.Input;

namespace CelestialLeague.Client.Renderer
{
    /*
    this is my terrible attempt at making a standalone ui renderer
    */
    public static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Console.WriteLine("=== Celestial League UI Renderer ===");
            Console.WriteLine();
            Console.WriteLine("This standalone renderer allows testing UI components");
            Console.WriteLine("outside of the main Celeste game environment.");
            Console.WriteLine();
            Console.WriteLine("Controls:");
            Console.WriteLine("   ESC      - Exit application");
            Console.WriteLine("   F1       - Show debug information");
            Console.WriteLine("   Tab      - Navigate between focusable elements");
            Console.WriteLine("   Mouse    - Click and hover interactions");
            Console.WriteLine();
            Console.WriteLine("Features being tested:");
            Console.WriteLine("   • Layout system (anchors, relative positioning)");
            Console.WriteLine("   • UI component hierarchy");
            Console.WriteLine("   • Mouse input handling");
            Console.WriteLine("   • Keyboard navigation");
            Console.WriteLine("   • Event system (click, hover, focus)");
            Console.WriteLine("   • Text rendering and alignment");
            Console.WriteLine("   • Color transitions and animations");
            Console.WriteLine("   • Real-time UI updates");
            Console.WriteLine();

            InitializeEnvironment();

            using var game = new UIRendererGame();
            game.Run();

            Console.WriteLine("Renderer shutdown complete.");
        }

        static void InitializeEnvironment()
        {
            Tags.Initialize();
            var mod = new CoreModule();
            mod._Settings = new CoreModuleSettings();
            CoreModule.Settings.DebugConsole = new ButtonBinding(0, Keys.OemPeriod);
            CoreModule.Settings.ToggleDebugConsole = new ButtonBinding(0, Keys.OemTilde);
        }
    }
}
 