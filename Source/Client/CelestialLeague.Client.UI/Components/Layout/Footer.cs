using Celeste;
using CelestialLeague.Client.UI;
using Microsoft.Xna.Framework;

namespace CelestialLeague.Client.UI.Components
{
    public class Footer : Panel
    {
        public Footer()
        {
            BackgroundColor = Color.White;

            PanelBorders.Top.Width = 4;
            PanelBorders.Top.Color = new Color(100, 80, 110, 255);

            Layout.Anchor = new Vector2(0, 1);
            Layout.Size = new DimensionUnit2(1f, 0, 0, 80);
            Layout.Position = new DimensionUnit2(0, 0, 1f, 0);

            var Motd = new Text
            {
                UseRichText = true,
                Content = "WELCOME TO [bold][scale=1.3]CELESTIAL LEAGUE[/scale][/bold]!",
                Font = Fonts.Get("Montserrat Regular"),
                AutoSize = true,
                TextScale = 0.5f,
                TextColor = new Color(188, 157, 215),
                BackgroundColor = Color.Transparent,
                Layout = new()
                {
                    Position = new DimensionUnit2(0.02f, 0, 0.5f, 0),
                    Anchor = new Vector2(0, 0.5f)
                }
            };
            Add(Motd);

            var Gradient = new Gradient();
            Gradient.SetColors(
                new GradientColorPoint(0, new Color(58, 46, 66)),
                new GradientColorPoint(1, new Color(91, 75, 104))
            );
            Gradient.Rotation = 270f;
            Add(Gradient);
        }

        protected override void UpdateSelf(InterfaceManager ui) { /* soon */ }

        protected override void RenderSelf(InterfaceManager ui)
        {
            base.RenderSelf(ui);
        }
    }
}