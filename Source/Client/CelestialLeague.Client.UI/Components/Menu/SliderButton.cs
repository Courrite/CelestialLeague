using Celeste.Mod;
using CelestialLeague.Client.UI;
using CelestialLeague.Client.Motion;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;

namespace CelestialLeague.Client.UI.Components
{
    public class SliderButton : Button
    {
        public new Color BackgroundColor
        {
            get => base.BackgroundColor;
            set
            {
                base.BackgroundColor = value;

                PanelBorders.Top.Color = Color.Multiply(value, 1.1f);
                PanelBorders.Left.Color = Color.Multiply(value, 0.9f);
                PanelBorders.Right.Color = Color.Multiply(value, 0.9f);
                PanelBorders.Bottom.Color = Color.Multiply(value, 0.8f);
            }
        }

        public HorizontalAlignment SpringTowards = HorizontalAlignment.Left;

        public float MinScale { get; set; } = 0.6f;
        public float MaxScale { get; set; } = 1.5f;
        public float RestScale { get; set; } = 1.0f;

        private Spring widthSpring = new Spring(1f);
        private Vector2 originalSize;

        public SliderButton()
        {
            Vector2 size = new Vector2(0.5f, 0);
            
            BackgroundColor = Color.White;
            Layout.RelativeSize = size;
            originalSize = size;
            Layout.AbsoluteSize = new Vector2(0, 125);

            OnMouseEnter += (component) => OnHoverEnter();
            OnMouseExit += (component) => OnHoverExit();
        }

        private void OnHoverEnter()
        {
            widthSpring.Target = MaxScale;
        }

        private void OnHoverExit()
        {
            widthSpring.Target = RestScale;
        }

        protected override void UpdateSelf(InterfaceManager ui)
        {
            widthSpring.Update(Engine.DeltaTime);
        }

        protected override void RenderSelf(InterfaceManager ui)
        {
            float scaleX = MathHelper.Clamp(widthSpring.Value, 0.01f, 10f);

            Vector2 scaledRelativeSize = new Vector2(originalSize.X * scaleX, originalSize.Y);

            Layout.RelativeSize = scaledRelativeSize;

            base.RenderSelf(ui);
        }
    }
}