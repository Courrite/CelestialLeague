using Microsoft.Xna.Framework;
using Monocle;
using System;
using Celeste;
using CelestialLeague.Client.UI.Types;
using Celeste.Mod;

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

                PanelBorders.Top.Color = Color.Multiply(value, 1.3f);
                PanelBorders.Left.Color = Color.Multiply(value, 1.15f);
                PanelBorders.Right.Color = Color.Multiply(value, 1.15f);
                PanelBorders.Bottom.Color = Color.Multiply(value, 0.5f);

                var Title = FindChild<Text>("Title");
                Title.TextColor = Color.Multiply(value, 2);

                var Description = FindChild<Text>("Description");
                Description.TextColor = Color.Multiply(value, 2);
            }
        }

        public HorizontalAlignment SpringTowards = HorizontalAlignment.Left;

        public float MinScale { get; set; } = 0.6f;
        public float MaxScale { get; set; } = 1.5f;
        public float RestScale { get; set; } = 1.0f;

        private Motion.Spring widthSpring = new(1f);
        private DimensionUnit2 originalSize;

        public SliderButton(string Title, string Description, MTexture Image = null)
        {
            Image ??= GFX.Gui["celestialleague/placeholder"];

            Image ButtonImage = new(Image)
            {
                Name = "Image",
                BackgroundColor = Color.Transparent,
                Layout = new()
                {
                    Size = new DimensionUnit2(0, 125, 0, 125),
                }
            };
            Add(ButtonImage);

            AspectRatio ImageAspectRatio = new()
            {
                Name = "AspectRatio",
                ConstrainAxis = ConstraintAxis.Height
            };
            ButtonImage.Add(ImageAspectRatio);

            Text TitleText = new(Title.ToUpper())
            {
                Name = "Title",
                Font = Fonts.Get("Montserrat Bold"),
                BackgroundColor = Color.Transparent,
                HorizontalAlignment = HorizontalAlignment.Left,
                AutoSize = false,
                Layout = new()
                {
                    Size = new DimensionUnit2(0.5f, 0, 0.5f, 0),
                    Position = new DimensionUnit2(0, 150, 0, 0)
                }
            };
            Add(TitleText);

            Text DescriptionText = new(Description)
            {
                Name = "Description",
                Font = Fonts.Get("Montserrat Regular"),
                BackgroundColor = Color.Transparent,
                TextColor = BackgroundColor,
                TextScale = 0.5f,
                AutoSize = false,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Layout = new()
                {
                    Size = new DimensionUnit2(0.5f, 0, 0.5f, 0),
                    Position = new DimensionUnit2(0, 150, 1, 0),
                    Anchor = new Vector2(0, 1)
                }
            };
            Add(DescriptionText);

            DimensionUnit2 size = new(0.5f, 0, 0, 125);

            Layout.Size = size;
            PanelBorders = new Borders(4);
            BackgroundColor = Color.White;

            OnMouseEnter += (component) => OnHoverEnter();
            OnMouseExit += (component) => OnHoverExit();

            originalSize = size;
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

            DimensionUnit2 scaledRelativeSize = new(originalSize.X.Scale * scaleX, 0, originalSize.Y.Scale, originalSize.Y.Offset);
            Layout.Size = scaledRelativeSize;

            base.RenderSelf(ui);
        }
    }
}