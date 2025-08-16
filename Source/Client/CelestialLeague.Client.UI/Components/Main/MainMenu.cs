using Celeste.Mod;
using CelestialLeague.Client.UI;
using Microsoft.Xna.Framework;

namespace CelestialLeague.Client.UI.Components
{
    public class MainMenu : Panel
    {
        public MainMenu()
        {
            Layout.RelativeSize = new Vector2(1f, 1f);
            BackgroundColor = Color.Black;

            var Footer = new Footer();
            Add(Footer);

            var Topbar = new Topbar();
            Add(Topbar);

            // var Scroll = new Scroll();
            // Scroll.BackgroundColor = Color.Transparent;
            // Scroll.Layout.RelativeSize = new Vector2(1f, 0.85f);
            // Scroll.Layout.RelativePosition = new Vector2(1f, 0.5f);
            // Scroll.Layout.Anchor = Anchor.MiddleRight;
            // Add(Scroll);

            // var ListLayout = new ListLayout();
            // ListLayout.VerticalAlignment = VerticalAlignment.Top;
            // ListLayout.HorizontalAlignment = HorizontalAlignment.Right;
            // Scroll.Add(ListLayout);

            // var Button1 = new SliderButton();
            // Button1.BackgroundColor = Color.Salmon;
            // Scroll.Add(Button1);

            // var Button2 = new SliderButton();
            // Button1.BackgroundColor = Color.Green;
            // Scroll.Add(Button2);

            // var Button3 = new SliderButton();
            // Button1.BackgroundColor = Color.Blue;
            // Scroll.Add(Button3);
        }

        protected override void UpdateSelf(InterfaceManager ui) { /* soon */ }

        protected override void RenderSelf(InterfaceManager ui)
        {
            base.RenderSelf(ui);
        }
    }
}