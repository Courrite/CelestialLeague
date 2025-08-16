using Celeste;
using CelestialLeague.Client.UI;
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

            Layout.Anchor = Anchor.TopLeft;
            Layout.RelativeSize = new Vector2(1f, 0);
            Layout.AbsoluteSize = new Vector2(0, 80);

            var ActiveMenu = new Text();
            ActiveMenu.Content = "MAIN MENU";
            ActiveMenu.TextColor = new Color(188, 157, 215);
            ActiveMenu.AutoSize = true;
            ActiveMenu.BackgroundColor = Color.Transparent;
            ActiveMenu.Layout.RelativePosition = new Vector2(0.02f, 0.5f);
            ActiveMenu.Layout.Anchor = Anchor.MiddleLeft;
            ActiveMenu.Font = Fonts.Get("Montserrat Regular");
            Add(ActiveMenu);

            var Gradient = new Gradient();
            Gradient.SetColors(
                new GradientColorPoint(0, new Color(58, 46, 66)),
                new GradientColorPoint(1, new Color(91, 75, 104))
            );
            Gradient.Rotation = 90f;
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