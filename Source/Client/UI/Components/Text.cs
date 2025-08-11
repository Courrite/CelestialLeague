using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using CelestialLeague.Client.UI.Core;
using Celeste.Mod;
using System.Linq;

namespace CelestialLeague.Client.UI.Components
{
    public class Text : Panel
    {
        private string text = "";
        private PixelFont font;
        private Vector2 measuredSize;
        private bool sizeDirty = true;
        private float textScale = 1.0f;

        private PixelFontSize cachedPixelFontSize = null;

        public string Content
        {
            get => text;
            set
            {
                if (text != value)
                {
                    text = value ?? "";
                    sizeDirty = true;
                    InvalidateLayout();
                }
            }
        }

        public PixelFont Font
        {
            get => font;
            set
            {
                if (font != value)
                {
                    font = value;
                    cachedPixelFontSize = null;
                    sizeDirty = true;
                    InvalidateLayout();
                }
            }
        }

        public float TextScale
        {
            get => textScale;
            set
            {
                if (Math.Abs(textScale - value) > 0.001f)
                {
                    textScale = value;
                    sizeDirty = true;
                    InvalidateLayout();
                }
            }
        }

        public Color TextColor { get; set; } = Color.White;
        public float TextTransparency { get; set; } = 0.0f;
        public TextAlignment Alignment { get; set; } = TextAlignment.Left;
        public bool WordWrap { get; set; } = false;
        public bool AutoSize { get; set; } = true;
        public float LineSpacing { get; set; } = 1.0f;

        public Text() : this("")
        {
            Layout.AbsoluteSize = new Vector2(50, 50);
        }

        public Text(string content) : base()
        {
            Content = content;
            CanReceiveFocus = false;
        }

        protected override void UpdateSelf(InterfaceManager ui)
        {
            if (sizeDirty)
            {
                RecalculateSize();
                sizeDirty = false;
            }
        }

        protected override void RenderSelf(InterfaceManager ui)
        {
            base.RenderSelf(ui);

            if (string.IsNullOrEmpty(text) || font == null) return;

            var bounds = GetWorldBounds();
            var contentBounds = GetContentBounds(bounds);

            Color renderColor = TextColor * (1.0f - TextTransparency);

            if (WordWrap && contentBounds.Width > 0)
            {
                RenderWrappedText(ui, contentBounds, renderColor);
            }
            else
            {
                RenderSingleLineText(ui, contentBounds, renderColor);
            }
        }

        private Rectangle GetContentBounds(Rectangle bounds)
        {
            var padding = Layout.Padding;
            return new Rectangle(
                bounds.X + (int)padding.Left,
                bounds.Y + (int)padding.Top,
                bounds.Width - (int)(padding.Left + padding.Right),
                bounds.Height - (int)(padding.Top + padding.Bottom)
            );
        }

        private void RenderSingleLineText(InterfaceManager ui, Rectangle contentBounds, Color renderColor)
        {
            Vector2 textSize = MeasureText(text) * TextScale;
            Vector2 position = CalculateTextPosition(contentBounds, textSize);

            DrawText(ui, text, position, renderColor);
        }

        private void RenderWrappedText(InterfaceManager ui, Rectangle contentBounds, Color renderColor)
        {
            var lines = WrapText(text, (int)(contentBounds.Width / TextScale));
            float lineHeight = GetLineHeight() * LineSpacing * TextScale;

            for (int i = 0; i < lines.Length; i++)
            {
                if (string.IsNullOrEmpty(lines[i])) continue;

                Vector2 lineSize = MeasureText(lines[i]) * TextScale;
                Vector2 linePosition = new Vector2(
                    CalculateHorizontalAlignment(contentBounds, lineSize.X),
                    contentBounds.Y + (i * lineHeight)
                );

                if (linePosition.Y + lineHeight <= contentBounds.Bottom)
                {
                    DrawText(ui, lines[i], linePosition, renderColor);
                }
            }
        }

        private PixelFontSize GetPixelFontSize()
        {
            if (cachedPixelFontSize != null)
                return cachedPixelFontSize;

            if (font == null)
                return null;

            try
            {
                if (font.Sizes?.Count > 0)
                {
                    cachedPixelFontSize = font.Sizes.First();
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, "CelestialLeague", $"Failed to get PixelFont size: {ex.Message}");
            }

            return cachedPixelFontSize;
        }

        private Vector2 MeasureText(string text)
        {
            if (string.IsNullOrEmpty(text)) return Vector2.Zero;

            try
            {
                var pixelFontSize = GetPixelFontSize();
                return pixelFontSize?.Measure(text) ?? Vector2.Zero;
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, "CelestialLeague", $"Failed to measure text with PixelFont: {ex.Message}");
                return Vector2.Zero;
            }
        }

        private float GetLineHeight()
        {
            try
            {
                var pixelFontSize = GetPixelFontSize();
                return pixelFontSize?.LineHeight ?? 0f;
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, "CelestialLeague", $"Failed to get line height with PixelFont: {ex.Message}");
                return 0f;
            }
        }

        private void DrawText(InterfaceManager ui, string text, Vector2 position, Color color)
        {
            if (string.IsNullOrEmpty(text) || font == null) return;

            try
            {
                var pixelFontSize = GetPixelFontSize();
                if (pixelFontSize != null)
                {
                    font.Draw(pixelFontSize.Size, text, position, Vector2.Zero, Vector2.One * TextScale, color);
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, "CelestialLeague", $"Failed to draw text with PixelFont: {ex.Message}");
            }
        }

        private Vector2 CalculateTextPosition(Rectangle bounds, Vector2 textSize)
        {
            float x = CalculateHorizontalAlignment(bounds, textSize.X);
            float y = bounds.Y + (bounds.Height - textSize.Y) * 0.5f;

            return new Vector2(x, y);
        }

        private float CalculateHorizontalAlignment(Rectangle bounds, float textWidth)
        {
            return Alignment switch
            {
                TextAlignment.Left => bounds.X,
                TextAlignment.Center => bounds.X + (bounds.Width - textWidth) * 0.5f,
                TextAlignment.Right => bounds.X + bounds.Width - textWidth,
                _ => bounds.X
            };
        }

        private string[] WrapText(string text, int maxWidth)
        {
            if (maxWidth <= 0) return new[] { text };

            var lines = new System.Collections.Generic.List<string>();
            var words = text.Split(' ');
            var currentLine = "";

            foreach (var word in words)
            {
                var testLine = string.IsNullOrEmpty(currentLine) ? word : currentLine + " " + word;
                var testSize = MeasureText(testLine);

                if (testSize.X <= maxWidth)
                {
                    currentLine = testLine;
                }
                else
                {
                    if (!string.IsNullOrEmpty(currentLine))
                    {
                        lines.Add(currentLine);
                        currentLine = word;
                    }
                    else
                    {
                        lines.Add(word);
                        currentLine = "";
                    }
                }
            }

            if (!string.IsNullOrEmpty(currentLine))
            {
                lines.Add(currentLine);
            }

            return lines.ToArray();
        }

        private void RecalculateSize()
        {
            if (string.IsNullOrEmpty(text) || font == null)
            {
                measuredSize = Vector2.Zero;
                return;
            }

            try
            {
                if (WordWrap && Layout.AbsoluteSize.HasValue)
                {
                    var contentWidth = Layout.AbsoluteSize.Value.X - Layout.Padding.Left - Layout.Padding.Right;
                    var lines = WrapText(text, (int)(contentWidth / TextScale));
                    float totalHeight = lines.Length * GetLineHeight() * LineSpacing * TextScale;
                    measuredSize = new Vector2(contentWidth, totalHeight);
                }
                else
                {
                    measuredSize = MeasureText(text) * TextScale;
                }

                if (AutoSize && !Layout.AbsoluteSize.HasValue && !Layout.RelativeSize.HasValue)
                {
                    Layout.AbsoluteSize = measuredSize + new Vector2(
                        Layout.Padding.Left + Layout.Padding.Right,
                        Layout.Padding.Top + Layout.Padding.Bottom
                    );
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "CelestialLeague", $"Failed to recalculate text size: {ex.Message}");
                measuredSize = Vector2.Zero;
            }
        }

        public Vector2 GetTextSize()
        {
            if (sizeDirty)
            {
                RecalculateSize();
                sizeDirty = false;
            }
            return measuredSize;
        }
    }

    public enum TextAlignment
    {
        Left,
        Center,
        Right
    }
}