using CelestialLeague.Client.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace CelestialLeague.Client.UI.Components
{
    public class Gradient : UIComponent, IDisposable
    {
        public List<GradientColorPoint> ColorSequence { get; set; }
        public List<GradientAlphaPoint> TransparencySequence { get; set; }
        public float Rotation { get; set; } // degrees
        public Vector2 Offset { get; set; }

        private Texture2D cachedGradientTexture;
        private int cachedWidth;
        private int cachedHeight;
        private bool isDirty = true;

        private const int MAX_GRADIENT_RESOLUTION = 256;

        public override int RenderOrder => 1000;

        public Gradient()
        {
            ColorSequence = new List<GradientColorPoint>
            {
                new GradientColorPoint(0f, Color.White),
                new GradientColorPoint(1f, Color.Black)
            };
            TransparencySequence = new List<GradientAlphaPoint>
            {
                new GradientAlphaPoint(0f, 0f),
                new GradientAlphaPoint(0f, 0f)
            };
            Rotation = 90f;
            Offset = Vector2.Zero;
        }

        public override void InvalidateLayout()
        {
            base.InvalidateLayout();
            isDirty = true;
        }

        public void SetColors(params GradientColorPoint[] gradientColorPoints)
        {
            ColorSequence.Clear();
            if (gradientColorPoints.Length == 0) return;

            for (int i = 0; i < gradientColorPoints.Length; i++)
            {
                ColorSequence.Add(gradientColorPoints[i]);
            }
            InvalidateLayout();
        }

        public void SetGradient(Color startColor, Color endColor)
        {
            ColorSequence.Clear();
            ColorSequence.Add(new GradientColorPoint(0f, startColor));
            ColorSequence.Add(new GradientColorPoint(1f, endColor));
            InvalidateLayout();
        }

        public void SetTransparency(params float[] alphas)
        {
            TransparencySequence.Clear();
            if (alphas.Length == 0) return;

            for (int i = 0; i < alphas.Length; i++)
            {
                float time = i / (float)(alphas.Length - 1);
                TransparencySequence.Add(new GradientAlphaPoint(time, alphas[i]));
            }
            InvalidateLayout();
        }

        protected override void UpdateSelf(InterfaceManager interfaceManager)
        {
            /* na */
        }

        public override void Render(InterfaceManager ui)
        {
            if (!IsVisible || Parent == null) return;

            Rectangle targetBounds = Parent.GetWorldBounds();

            if (targetBounds.Width <= 0 || targetBounds.Height <= 0) return;

            int texWidth = Math.Min(targetBounds.Width, MAX_GRADIENT_RESOLUTION);
            int texHeight = Math.Min(targetBounds.Height, MAX_GRADIENT_RESOLUTION);

            if (isDirty || cachedGradientTexture == null ||
                cachedWidth != texWidth || cachedHeight != texHeight)
            {
                GenerateGradientTexture(ui.GraphicsDevice, texWidth, texHeight);
            }

            if (cachedGradientTexture != null)
            {
                ui.SpriteBatch.Draw(cachedGradientTexture, targetBounds, Color.White);
            }
        }

        protected override void RenderSelf(InterfaceManager interfaceManager)
        {
            // rendering is handled in the Render override
        }

        private void GenerateGradientTexture(GraphicsDevice graphicsDevice, int width, int height)
        {
            cachedGradientTexture?.Dispose();

            cachedGradientTexture = new Texture2D(graphicsDevice, width, height);
            Color[] pixelData = new Color[width * height];

            // calculate rotation in radians
            float rotRad = MathHelper.ToRadians(Rotation);
            float cosRot = (float)Math.Cos(rotRad);
            float sinRot = (float)Math.Sin(rotRad);

            // generate gradient pixels
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // normalize coordinates to 0-1 range
                    float nx = width > 1 ? (float)x / (width - 1) : 0f;
                    float ny = height > 1 ? (float)y / (height - 1) : 0f;

                    // apply offset
                    nx += Offset.X;
                    ny += Offset.Y;

                    // apply rotation (rotate around center)
                    float cx = nx - 0.5f;
                    float cy = ny - 0.5f;
                    float rotatedX = cx * cosRot - cy * sinRot + 0.5f;
                    float rotatedY = cx * sinRot + cy * cosRot + 0.5f;

                    // use X coordinate for gradient
                    float gradientPosition = MathHelper.Clamp(rotatedX, 0f, 1f);

                    Color baseColor = SampleGradient(gradientPosition);
                    float transparency = SampleTransparency(gradientPosition);

                    // create gradient effect - transparent areas let parent show through
                    Color finalColor = Color.Lerp(Color.Transparent, baseColor, 1f - transparency);

                    pixelData[y * width + x] = finalColor;
                }
            }

            cachedGradientTexture.SetData(pixelData);
            cachedWidth = width;
            cachedHeight = height;
            isDirty = false;
        }

        private Color SampleGradient(float position)
        {
            if (ColorSequence.Count == 0) return Color.White;
            if (ColorSequence.Count == 1) return ColorSequence[0].Color;

            // find surrounding points
            GradientColorPoint prev = ColorSequence[0];
            GradientColorPoint next = ColorSequence[ColorSequence.Count - 1];

            for (int i = 0; i < ColorSequence.Count - 1; i++)
            {
                if (position >= ColorSequence[i].Time && position <= ColorSequence[i + 1].Time)
                {
                    prev = ColorSequence[i];
                    next = ColorSequence[i + 1];
                    break;
                }
            }

            // handle edge cases
            if (position <= ColorSequence[0].Time) return ColorSequence[0].Color;
            if (position >= ColorSequence[ColorSequence.Count - 1].Time)
                return ColorSequence[ColorSequence.Count - 1].Color;

            // interpolate between points
            float t = (position - prev.Time) / (next.Time - prev.Time);
            return Color.Lerp(prev.Color, next.Color, t);
        }

        private float SampleTransparency(float position)
        {
            if (TransparencySequence.Count == 0) return 0f;
            if (TransparencySequence.Count == 1) return TransparencySequence[0].Alpha;

            // find surrounding points
            GradientAlphaPoint prev = TransparencySequence[0];
            GradientAlphaPoint next = TransparencySequence[TransparencySequence.Count - 1];

            for (int i = 0; i < TransparencySequence.Count - 1; i++)
            {
                if (position >= TransparencySequence[i].Time && position <= TransparencySequence[i + 1].Time)
                {
                    prev = TransparencySequence[i];
                    next = TransparencySequence[i + 1];
                    break;
                }
            }

            // handle edge cases
            if (position <= TransparencySequence[0].Time) return TransparencySequence[0].Alpha;
            if (position >= TransparencySequence[TransparencySequence.Count - 1].Time)
                return TransparencySequence[TransparencySequence.Count - 1].Alpha;

            // interpolate between points
            float t = (position - prev.Time) / (next.Time - prev.Time);
            return MathHelper.Lerp(prev.Alpha, next.Alpha, t);
        }

        public void Dispose()
        {
            cachedGradientTexture?.Dispose();
            cachedGradientTexture = null;
        }
    }

    public struct GradientColorPoint
    {
        public float Time { get; set; }
        public Color Color { get; set; }

        public GradientColorPoint(float time, Color color)
        {
            Time = MathHelper.Clamp(time, 0f, 1f);
            Color = color;
        }
    }

    public struct GradientAlphaPoint
    {
        public float Time { get; set; }
        public float Alpha { get; set; }

        public GradientAlphaPoint(float time, float alpha)
        {
            Time = MathHelper.Clamp(time, 0f, 1f);
            Alpha = MathHelper.Clamp(alpha, 0f, 1f);
        }
    }
}