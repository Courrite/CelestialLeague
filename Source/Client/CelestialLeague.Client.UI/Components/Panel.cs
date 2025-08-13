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
        private Color backgroundRgba => new Color(BackgroundColor.R, BackgroundColor.G, BackgroundColor.B, (int)((1.0f - BackgroundTransparency) * 255));
        private Color borderRgba => new Color(BorderColor.R, BorderColor.G, BorderColor.B, (int)((1.0f - BorderTransparency) * 255));

        public Color BackgroundColor { get; set; } = Color.DarkGray;
        public Color BorderColor { get; set; } = Color.Black;
        
        public float BackgroundTransparency { get; set; } = 0.0f;
        public float BorderTransparency { get; set; } = 0.0f;
        
        public int BorderWidth { get; set; } = 1;

        public Panel()
        {
            Layout.RelativeSize = new Vector2(0.1f, 0.1f);
        }

        protected override void UpdateSelf(InterfaceManager ui)
        {
            // this is a container so it doesnt need updates
        }

        protected override void RenderSelf(InterfaceManager ui)
        {
            var bounds = GetWorldBounds();

            if (BackgroundTransparency < 1.0f)
            {
                ui.DrawRectangle(bounds, backgroundRgba);
            }

            if (BorderTransparency < 1.0f && BorderWidth > 0)
            {
                ui.DrawRectangleOutline(bounds, borderRgba, BorderWidth);
            }
        }
    }
}