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
        public Color BorderColor { get; set; } = Color.Black;

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

            if (BackgroundColor.A > 0)
            {
                ui.DrawRectangle(bounds, BackgroundColor);
            }

            if (BorderColor.A > 0 && BorderWidth > 0)
            {
                Rectangle borderBounds = new Rectangle(
                    bounds.X - BorderWidth,
                    bounds.Y - BorderWidth,
                    bounds.Width + (BorderWidth * 2),
                    bounds.Height + (BorderWidth * 2)
                );

                ui.DrawRectangleOutline(borderBounds, BorderColor, BorderWidth);
            }
        }

    }
}