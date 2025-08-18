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
            LayoutHelper.DebugLayout(this, "Main Menu");

            var Footer = new Footer();
            Footer.Name = "Footer";
            Add(Footer);
            LayoutHelper.DebugLayout(Footer, "Footer");

            var Topbar = new Topbar();
            Topbar.Name = "Topbar";
            Add(Topbar);
            LayoutHelper.DebugLayout(Topbar, "Topbar");

            var ButtonContainerPanel = new Panel();
            ButtonContainerPanel.Name = "ButtonContainerPanel";
            ButtonContainerPanel.BackgroundColor = Color.Transparent;
            ButtonContainerPanel.Layout.Size = new DimensionUnit2(1f, 0, 0.85f, 0);
            ButtonContainerPanel.Layout.Position = new DimensionUnit2(1f, 0, 0.5f, 0);
            ButtonContainerPanel.Layout.Anchor = new Vector2(1, 0.5f);
            Add(ButtonContainerPanel);
            LayoutHelper.DebugLayout(ButtonContainerPanel, "Button Container");

            var ListLayout = new ListLayout();
            ListLayout.Name = "ListLayout";
            ListLayout.VerticalAlignment = VerticalAlignment.Top;
            ListLayout.HorizontalAlignment = HorizontalAlignment.Right;
            ListLayout.Padding = new Vector2(0, 15);
            ButtonContainerPanel.Add(ListLayout);

            var Button1 = new SliderButton();
            Button1.Name = "Button1";
            Button1.BackgroundColor = Color.Salmon;
            ButtonContainerPanel.Add(Button1);
            LayoutHelper.DebugLayout(Button1, "Button 1");

            var Button2 = new SliderButton();
            Button2.Name = "Button2";
            Button2.BackgroundColor = Color.Green;
            ButtonContainerPanel.Add(Button2);
            LayoutHelper.DebugLayout(Button2, "Button 2");

            var Button3 = new SliderButton();
            Button3.Name = "Button3";
            Button3.BackgroundColor = Color.Blue;
            ButtonContainerPanel.Add(Button3);
            LayoutHelper.DebugLayout(Button3, "Button 3");
        }

        protected override void UpdateSelf(InterfaceManager ui) { /* soon */ }

        protected override void RenderSelf(InterfaceManager ui)
        {
            base.RenderSelf(ui);
        }
    }
}