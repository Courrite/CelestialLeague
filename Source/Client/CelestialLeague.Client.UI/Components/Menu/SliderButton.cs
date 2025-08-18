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
        private DimensionUnit2 originalSize;

        public SliderButton()
        {
            DimensionUnit2 size = new DimensionUnit2(0.5f, 0, 0, 125);

            BackgroundColor = Color.White;
            Layout.Size = size;
            originalSize = size;

            OnMouseEnter += (component) => OnHoverEnter();
            OnMouseExit += (component) => OnHoverExit();

            widthSpring.Stiffness = 4f;
            widthSpring.Damping = (float)(2 * Math.Sqrt(widthSpring.Stiffness));
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

            DimensionUnit2 scaledRelativeSize = new DimensionUnit2(originalSize.X.Scale * scaleX, 0, originalSize.Y.Scale, originalSize.Y.Offset);
            Layout.Size = scaledRelativeSize;

            base.RenderSelf(ui);
        }
    }
}