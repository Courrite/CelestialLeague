using Celeste.Mod;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using CelestialLeague.Client.Motion;
using Monocle;
using CelestialLeague.Client.UI.Core;

namespace CelestialLeague.Client.UI.Components
{
    public class Panel : UIComponent
    {
        public Color BackgroundColor { get; set; } = Color.DarkGray;
        public bool DrawBackground { get; set; } = true;
        public bool DrawBorder { get; set; } = true;

        public Panel()
        {
            Layout.AbsoluteSize = new Vector2(50, 50);
        }

        protected override void UpdateSelf(InterfaceManager ui)
        {
            // this is a container so we dont do anything here
        }

        protected override void RenderSelf(InterfaceManager ui)
        {
            var bounds = GetWorldBounds();

            if (DrawBackground)
            {
                Color fadedBg = BackgroundColor * BackgroundTransparency;
                ui.DrawRectangle(bounds, fadedBg);
            }

            if (DrawBorder)
            {
                Color fadedBorder = Color.Black * BackgroundTransparency;
                ui.DrawRectangleOutline(bounds, fadedBorder, 1);
            }
        }
    }

}