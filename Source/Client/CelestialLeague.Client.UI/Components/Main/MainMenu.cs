using Celeste.Mod;
using CelestialLeague.Client.UI;
using Microsoft.Xna.Framework;

namespace CelestialLeague.Client.UI.Components
{
    public class MainMenu : Panel
    {
        public MainMenu()
        {
            Name = "MainMenu";
            Layout.Size = new DimensionUnit2(1f, 0, 1f, 0);
            BackgroundColor = Color.Black;

            var Footer = new Footer
            {
                Name = "Footer"
            };
            Add(Footer);

            var Topbar = new Topbar
            {
                Name = "Topbar"
            };
            Add(Topbar);

            var ButtonContainerPanel = new Panel
            {
                Name = "ButtonContainerPanel",
                BackgroundColor = Color.Transparent,
                Layout = new()
                {
                    Size = new DimensionUnit2(1f, 0, 0.85f, 0),
                    Position = new DimensionUnit2(1f, 0, 0.5f, 0),
                    Anchor = new Vector2(1, 0.5f),
                }
            };
            Add(ButtonContainerPanel);

            var ListLayout = new ListLayout
            {
                Name = "ListLayout",
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Right,
                Padding = new Vector2(0, 15)
            };
            ButtonContainerPanel.Add(ListLayout);

            var Button1 = new SliderButton("TEST 1", "This is test button number 1")
            {
                Name = "Button1",
                BackgroundColor = new(122, 0, 0)
            };
            ButtonContainerPanel.Add(Button1);

            var Button2 = new SliderButton("TEST 2", "This is a test button numbered 2")
            {
                Name = "Button2",
                BackgroundColor = new(0, 122, 0)
            };
            ButtonContainerPanel.Add(Button2);

            var Button3 = new SliderButton("TEST 3", "This is Test button No. 3")
            {
                Name = "Button3",
                BackgroundColor = new(0, 0, 122)
            };
            ButtonContainerPanel.Add(Button3);
        }

        protected override void UpdateSelf(InterfaceManager ui) { /* soon */ }

        protected override void RenderSelf(InterfaceManager ui)
        {
            base.RenderSelf(ui);
        }
    }
}