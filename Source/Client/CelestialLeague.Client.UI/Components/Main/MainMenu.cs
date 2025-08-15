using CelestialLeague.Client.UI.Core;
using Microsoft.Xna.Framework;

namespace CelestialLeague.Client.UI.Components
{
    public class MainMenu : Panel
    {
        public MainMenu()
        {
            Layout.RelativeSize = new Vector2(1f, 1f);
            BackgroundTransparency = 1f;

            var Footer = new Footer();
            Add(Footer);

            var Topbar = new Topbar();
            Add(Topbar);
        }

        protected override void UpdateSelf(InterfaceManager ui) { /* soon */ }

        protected override void RenderSelf(InterfaceManager ui)
        {
            base.RenderSelf(ui);
        }
    }
}