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

            var Motd = new Text();
            Motd.UseRichText = true;
            Motd.Content = "WELCOME TO [bold][scale=1.3]CELESTIAL LEAGUE[/scale][/bold]!";
            Motd.AutoSize = true;
            Motd.TextScale = 0.5f;
            Motd.TextColor = new Color(188, 157, 215);
            Motd.BackgroundColor = Color.Transparent;
            Motd.Layout.Position = new DimensionUnit2(0.02f, 0, 0.5f, 0);
            Motd.Layout.Anchor = new Vector2(0, 0.5f);
            Motd.Font = Fonts.Get("Montserrat Regular");
            Add(Motd);

            var Gradient = new Gradient();
            Gradient.SetColors(
                new GradientColorPoint(0, new Color(58, 46, 66)),
                new GradientColorPoint(1, new Color(91, 75, 104))
            );
            Gradient.Rotation = 270f;
            Add(Gradient);
            
            LayoutHelper.DebugLayout(this);
        }

        protected override void UpdateSelf(InterfaceManager ui) { /* soon */ }

        protected override void RenderSelf(InterfaceManager ui)
        {
            base.RenderSelf(ui);
        }
    }
}