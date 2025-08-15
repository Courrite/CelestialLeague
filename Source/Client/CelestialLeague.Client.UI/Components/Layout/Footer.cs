using Celeste;
using CelestialLeague.Client.UI.Core;
using Microsoft.Xna.Framework;

namespace CelestialLeague.Client.UI.Components
{
    public class Footer : Panel
    {
        public Footer()
        {
            BackgroundColor = Color.White;
            Layout.Anchor = Anchor.BottomLeft;
            Layout.RelativeSize = new Vector2(1f, 0);
            Layout.AbsoluteSize = new Vector2(0, 80);
            Layout.RelativePosition = new Vector2(0, 1f);

            var Motd = new Text();
            Motd.Content = "Welcome to Celestial League!";
            Motd.AutoSize = true;
            Motd.BorderTransparency = 1;
            Motd.BackgroundTransparency = 1;
            Motd.Layout.RelativePosition = new Vector2(0.02f, 0.5f);
            Motd.Layout.Anchor = Anchor.MiddleLeft;
            Motd.Font = Fonts.Get("Montserrat Regular");
            Add(Motd);

            var Gradient = new Gradient();
            Gradient.SetColors(
                new GradientColorPoint(0, Color.Black),
                new GradientColorPoint(1, new Color(224, 176, 255))
            );
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