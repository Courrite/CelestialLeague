using Celeste;
using CelestialLeague.Client.UI;
using CelestialLeague.Client.UI.Types;
using Microsoft.Xna.Framework;

namespace CelestialLeague.Client.UI.Components
{
    public class Topbar : Panel
    {
        public Topbar()
        {
            BackgroundColor = Color.White;

            PanelBorders.Bottom.Width = 4;
            PanelBorders.Bottom.Color = new Color(100, 80, 110, 255);

            Layout.Anchor = Vector2.Zero;
            Layout.Size = new DimensionUnit2(1f, 0, 0, 80);

            var ActiveMenu = new Text
            {
                Content = "MAIN MENU",
                TextColor = new Color(188, 157, 215),
                AutoSize = true,
                Font = Fonts.Get("Montserrat Regular"),
                BackgroundColor = Color.Transparent,
                Layout = new()
                {
                    Position = new DimensionUnit2(0.02f, 0, 0.5f, 0),
                    Anchor = new Vector2(0, 0.5f)
                }
            };
            Add(ActiveMenu);

            var Gradient = new Gradient();
            Gradient.SetColors(
                new GradientColorPoint(0, new Color(58, 46, 66)),
                new GradientColorPoint(1, new Color(91, 75, 104))
            );
            Gradient.Rotation = 90f;
            Add(Gradient);
        }

        protected override void UpdateSelf(InterfaceManager ui) { /* soon */ }

        protected override void RenderSelf(InterfaceManager ui)
        {
            base.RenderSelf(ui);
        }
    }
}