using Celeste;
using CelestialLeague.Client.UI.Core;
using Microsoft.Xna.Framework;

namespace CelestialLeague.Client.UI.Components
{
    public class Topbar : Panel
    {
        public Topbar()
        {
            BackgroundColor = new Color(0, 0, 0);
            Layout.Anchor = Anchor.TopLeft;
            Layout.RelativeSize = new Vector2(1f, 0);
            Layout.AbsoluteSize = new Vector2(0, 80);

            // var ActiveMenu = new Text();
            // ActiveMenu.Content = "MAIN MENU";
            // ActiveMenu.AutoSize = true;
            // ActiveMenu.BorderTransparency = 1;
            // ActiveMenu.BackgroundTransparency = 1;
            // ActiveMenu.Layout.RelativePosition = new Vector2(0.02f, 0.5f);
            // ActiveMenu.Layout.Anchor = Anchor.MiddleLeft;
            // ActiveMenu.Font = Fonts.Get("Montserrat Regular");
            // Add(ActiveMenu);

            LayoutHelper.DebugLayout(this);
        }

        protected override void UpdateSelf(InterfaceManager ui) { /* soon */ }

        protected override void RenderSelf(InterfaceManager ui)
        {
            base.RenderSelf(ui);
        }
    }
}