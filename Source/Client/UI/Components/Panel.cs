using Celeste.Mod;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using CelestialLeague.Client.Motion;
using Monocle;
using CelestialLeague.Client.UI.Core;

namespace CelestialLeague.Client.UI.Components
{
    public class Panel : UIComponent
    {
        public Color BackgroundColor { get; set; } = Color.DarkGray;
        public bool DrawBackground { get; set; } = true;
        public bool DrawBorder { get; set; } = true;

        private Spring xSpring;
        private Spring fadeSpring;
        private bool wasTabPressed = false;

        public Panel()
        {
            Layout.RelativeSize = new Vector2(1, 1);

            xSpring = new Spring(0f);
            xSpring.Stiffness = 120f;
            xSpring.Damping = 25f;

            fadeSpring = new Spring(1f);
            fadeSpring.Stiffness = 150f;
            fadeSpring.Damping = 20f;
        }

        protected override void UpdateSelf(InterfaceManager ui)
        {
            bool tabPressed = MInput.Keyboard.Check(Keys.Tab);
            if (tabPressed && !wasTabPressed)
            {
                if (xSpring.Target == 0f)
                {
                    xSpring.Target = 800f;
                    fadeSpring.Target = 0f;
                }
                else
                {
                    xSpring.Target = 0f;
                    fadeSpring.Target = 1f;
                }
            }
            wasTabPressed = tabPressed;

            xSpring.Update(Engine.DeltaTime);
            fadeSpring.Update(Engine.DeltaTime);
        }

        protected override void RenderSelf(InterfaceManager ui)
        {
            var baseBounds = GetWorldBounds();
            
            Rectangle animatedBounds = new Rectangle(
                baseBounds.X + (int)xSpring.Value, 
                baseBounds.Y, 
                baseBounds.Width, 
                baseBounds.Height
            );
            
            float alpha = fadeSpring.Value;


            if (DrawBackground)
            {
                Color fadedBg = BackgroundColor * alpha;
                ui.DrawRectangle(animatedBounds, fadedBg);
            }

            if (DrawBorder)
            {
                Color fadedBorder = Color.Black * alpha;
                ui.DrawRectangleOutline(animatedBounds, fadedBorder, 1);
            }
        }
    }
}