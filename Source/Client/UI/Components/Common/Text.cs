using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CelestialLeague.Client.UI;
using Celeste.Mod;
using Celeste;
using CelestialLeague.Client.UI.Types;

namespace CelestialLeague.Client.UI.Components
{
    public struct RichTextSegment(string text, Color color = default, TextStyle style = TextStyle.Normal, float scale = 1.0f)
    {
        public string Text = text ?? "";
        public Color Color = color == default ? Color.White : color;
        public TextStyle Style = style;
        public float Scale = scale;
        public Vector2 Position = Vector2.Zero;
        public Vector2 Size = Vector2.Zero;
    }

    public class Text : Panel
    {
        private string text = "";
        private PixelFont font;
        private Vector2 measuredSize;
        private bool sizeDirty = true;
        private float textScale = 1.0f;
        private PixelFontSize cachedPixelFontSize = null;

        // rich text properties
        private readonly List<RichTextSegment> segments = [];
        private bool segmentsDirty = true;
        private bool useRichText = false;

        public string Content
        {
            get => text;
            set
            {
                if (text != value)
                {
                    text = value ?? "";
                    sizeDirty = true;
                    segmentsDirty = true;
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

        public bool UseRichText
        {
            get => useRichText;
            set
            {
                if (useRichText != value)
                {
                    useRichText = value;
                    segmentsDirty = true;
                    sizeDirty = true;
                    InvalidateLayout();
                }
            }
        }

        public Color TextColor { get; set; } = Color.White;
        public float TextTransparency { get; set; } = 0.0f;
        public HorizontalAlignment HorizontalAlignment { get; set; } = HorizontalAlignment.Left;
        public VerticalAlignment VerticalAlignment { get; set; } = VerticalAlignment.Center;
        public bool WordWrap { get; set; } = false;
        public bool AutoSize { get; set; } = true;
        public float LineSpacing { get; set; } = 1.0f;

        public Text() : this("")
        {
        }

        public Text(string content) : base()
        {
            Content = content;
            CanReceiveFocus = false;
        }

        protected override void UpdateSelf(InterfaceManager ui)
        {
            if (useRichText && segmentsDirty)
            {
                ParseRichText();
                segmentsDirty = false;
                sizeDirty = true;
            }

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
            if (useRichText && segments.Count > 0)
            {
                if (WordWrap && bounds.Width > 0)
                {
                    RenderWrappedRichText(ui, bounds);
                }
                else
                {
                    RenderSingleLineRichText(ui, bounds);
                }
            }
            else
            {
                Color renderColor = TextColor * (1.0f - TextTransparency);

                if (WordWrap && bounds.Width > 0)
                {
                    RenderWrappedText(ui, bounds, renderColor);
                }
                else
                {
                    RenderSingleLineText(ui, bounds, renderColor);
                }
            }
        }

        #region Rich Text Methods

        private void ParseRichText()
        {
            segments.Clear();

            if (string.IsNullOrEmpty(text))
                return;

            try
            {
                var parsedSegments = ParseTextRecursively(text, TextColor, TextStyle.Normal, 1.0f);
                segments.AddRange(parsedSegments);
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, "Celestial League", $"Failed to parse rich text: {ex.Message}");
                segments.Add(new RichTextSegment(text, TextColor, TextStyle.Normal, 1.0f));
            }
        }

        private List<RichTextSegment> ParseTextRecursively(string text, Color currentColor, TextStyle currentStyle, float currentScale)
        {
            var result = new List<RichTextSegment>();
            var processedText = text;
            var currentPos = 0;
            var tagStack = new Stack<(Color color, TextStyle style, float scale)>();

            while (currentPos < processedText.Length)
            {
                int nextTagStart = processedText.IndexOf('[', currentPos);
                if (nextTagStart == -1)
                {
                    string remainingText = processedText.Substring(currentPos);
                    if (!string.IsNullOrEmpty(remainingText))
                    {
                        result.Add(new RichTextSegment(remainingText, currentColor, currentStyle, currentScale));
                    }
                    break;
                }

                if (nextTagStart > currentPos)
                {
                    string beforeText = processedText.Substring(currentPos, nextTagStart - currentPos);
                    if (!string.IsNullOrEmpty(beforeText))
                    {
                        result.Add(new RichTextSegment(beforeText, currentColor, currentStyle, currentScale));
                    }
                }

                int tagEnd = processedText.IndexOf(']', nextTagStart);
                if (tagEnd == -1)
                {
                    result.Add(new RichTextSegment(processedText.Substring(nextTagStart), currentColor, currentStyle, currentScale));
                    break;
                }

                string tagContent = processedText.Substring(nextTagStart + 1, tagEnd - nextTagStart - 1);

                if (tagContent.StartsWith("/"))
                {
                    if (tagStack.Count > 0)
                    {
                        var (prevColor, prevStyle, prevScale) = tagStack.Pop();
                        currentColor = prevColor;
                        currentStyle = prevStyle;
                        currentScale = prevScale;
                    }
                }
                else
                {
                    tagStack.Push((currentColor, currentStyle, currentScale));

                    if (tagContent.StartsWith("color="))
                    {
                        string colorValue = tagContent.Substring(6);
                        currentColor = ParseColor(colorValue);
                    }
                    else if (tagContent.StartsWith("scale="))
                    {
                        string scaleValue = tagContent.Substring(6);
                        if (float.TryParse(scaleValue, out float scale))
                        {
                            currentScale = scale;
                        }
                    }
                    else
                    {
                        currentStyle = ParseStyle(tagContent);
                    }
                }

                currentPos = tagEnd + 1;
            }

            return result;
        }

        private Color ParseColor(string colorStr)
        {
            try
            {
                var namedColors = new Dictionary<string, Color>(StringComparer.OrdinalIgnoreCase)
                {
                    {"red", Color.Red}, {"green", Color.Green}, {"blue", Color.Blue},
                    {"white", Color.White}, {"black", Color.Black}, {"yellow", Color.Yellow},
                    {"orange", Color.Orange}, {"purple", Color.Purple}, {"pink", Color.Pink},
                    {"gray", Color.Gray}, {"grey", Color.Gray}
                };

                if (namedColors.TryGetValue(colorStr, out Color namedColor))
                    return namedColor;

                if (colorStr.StartsWith("#"))
                    colorStr = colorStr.Substring(1);

                if (colorStr.Length == 3)
                {
                    colorStr = $"{colorStr[0]}{colorStr[0]}{colorStr[1]}{colorStr[1]}{colorStr[2]}{colorStr[2]}";
                }

                if (colorStr.Length == 6 && int.TryParse(colorStr, System.Globalization.NumberStyles.HexNumber, null, out int colorValue))
                {
                    int r = (colorValue >> 16) & 0xFF;
                    int g = (colorValue >> 8) & 0xFF;
                    int b = colorValue & 0xFF;
                    return new Color(r, g, b);
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, "Celestial League", $"Failed to parse color '{colorStr}': {ex.Message}");
            }

            return TextColor;
        }

        private static TextStyle ParseStyle(string styleStr)
        {
            return styleStr.ToLower() switch
            {
                "underline" => TextStyle.Underline,
                "strike" => TextStyle.Strikethrough,
                "caps" => TextStyle.AllCaps,
                _ => TextStyle.Normal
            };
        }

        private static string ProcessTextStyle(string text, TextStyle style)
        {
            return style switch
            {

                TextStyle.AllCaps => text.ToUpper(),
                _ => text
            };
        }

        private void RenderSingleLineRichText(InterfaceManager ui, Rectangle contentBounds)
        {
            float totalWidth = segments.Sum(s => MeasureSegmentText(s.Text, s.Style, s.Scale * TextScale).X);
            float maxHeight = segments.Max(s => MeasureSegmentText(s.Text, s.Style, s.Scale * TextScale).Y);
            Vector2 totalSize = new(totalWidth, maxHeight);

            Vector2 startPosition = CalculateTextRenderPosition(contentBounds, totalSize);
            float currentX = startPosition.X;

            foreach (var segment in segments)
            {
                if (string.IsNullOrEmpty(segment.Text)) continue;

                string processedText = ProcessTextStyle(segment.Text, segment.Style);
                Vector2 segmentSize = MeasureSegmentText(processedText, segment.Style, segment.Scale * TextScale);

                Color renderColor = segment.Color * (1.0f - TextTransparency);

                Vector2 position = new Vector2(currentX, startPosition.Y);

                DrawStyledText(ui, processedText, position, renderColor, segment.Style, segment.Scale * TextScale);
                currentX += segmentSize.X;
            }
        }

        private void RenderWrappedRichText(InterfaceManager ui, Rectangle contentBounds)
        {
            var lines = WrapRichText(segments, contentBounds.Width);
            float lineHeight = GetLineHeight() * LineSpacing * TextScale;
            float totalTextHeight = lines.Count * lineHeight;

            float startY = VerticalAlignment switch
            {
                VerticalAlignment.Top => contentBounds.Y,
                VerticalAlignment.Center => contentBounds.Y + (contentBounds.Height - totalTextHeight) * 0.5f,
                VerticalAlignment.Bottom => contentBounds.Y + contentBounds.Height - totalTextHeight,
                _ => contentBounds.Y
            };

            for (int i = 0; i < lines.Count; i++)
            {
                float currentY = startY + (i * lineHeight);
                if (currentY + lineHeight > contentBounds.Bottom) break;

                RenderRichTextLine(ui, lines[i], contentBounds, currentY);
            }
        }

        private void RenderRichTextLine(InterfaceManager ui, List<RichTextSegment> lineSegments, Rectangle contentBounds, float y)
        {
            float totalWidth = lineSegments.Sum(s => MeasureSegmentText(ProcessTextStyle(s.Text, s.Style), s.Style, s.Scale * TextScale).X);
            float currentX = HorizontalAlignment switch
            {
                HorizontalAlignment.Left => contentBounds.X,
                HorizontalAlignment.Center => contentBounds.X + (contentBounds.Width - totalWidth) * 0.5f,
                HorizontalAlignment.Right => contentBounds.X + contentBounds.Width - totalWidth,
                _ => contentBounds.X
            };

            foreach (var segment in lineSegments)
            {
                if (string.IsNullOrEmpty(segment.Text)) continue;

                string processedText = ProcessTextStyle(segment.Text, segment.Style);
                Color renderColor = segment.Color * (1.0f - TextTransparency);
                Vector2 position = new(currentX, y);

                DrawStyledText(ui, processedText, position, renderColor, segment.Style, segment.Scale * TextScale);
                currentX += MeasureSegmentText(processedText, segment.Style, segment.Scale * TextScale).X;
            }
        }

        private List<List<RichTextSegment>> WrapRichText(List<RichTextSegment> segments, float maxWidth)
        {
            var lines = new List<List<RichTextSegment>>();
            var currentLine = new List<RichTextSegment>();
            float currentLineWidth = 0f;

            foreach (var segment in segments)
            {
                var words = segment.Text.Split(' ');
                bool isFirstWord = true;

                foreach (var word in words)
                {
                    string processedWord = ProcessTextStyle(word, segment.Style);
                    var wordSegment = new RichTextSegment(word, segment.Color, segment.Style, segment.Scale);
                    float wordWidth = MeasureSegmentText(processedWord, segment.Style, segment.Scale * TextScale).X;

                    if (currentLine.Count > 0 && !isFirstWord)
                    {
                        var spaceWidth = MeasureSegmentText(" ", segment.Style, segment.Scale * TextScale).X;
                        if (currentLineWidth + spaceWidth + wordWidth > maxWidth)
                        {
                            lines.Add(currentLine);
                            currentLine = [wordSegment];
                            currentLineWidth = wordWidth;
                        }
                        else
                        {
                            currentLine.Add(new RichTextSegment(" ", segment.Color, segment.Style, segment.Scale));
                            currentLine.Add(wordSegment);
                            currentLineWidth += spaceWidth + wordWidth;
                        }
                    }
                    else
                    {
                        if (currentLineWidth + wordWidth > maxWidth && currentLine.Count > 0)
                        {
                            lines.Add(currentLine);
                            currentLine = [wordSegment];
                            currentLineWidth = wordWidth;
                        }
                        else
                        {
                            currentLine.Add(wordSegment);
                            currentLineWidth += wordWidth;
                        }
                    }
                    isFirstWord = false;
                }
            }

            if (currentLine.Count > 0)
            {
                lines.Add(currentLine);
            }

            return lines;
        }

        private Vector2 MeasureSegmentText(string text, TextStyle style, float scale)
        {
            if (string.IsNullOrEmpty(text)) return Vector2.Zero;

            try
            {
                var pixelFontSize = GetPixelFontSize();
                if (pixelFontSize == null) return Vector2.Zero;

                Vector2 baseSize = pixelFontSize.Measure(text);
                return baseSize * scale;
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, "Celestial League", $"Failed to measure segment text: {ex.Message}");
                return Vector2.Zero;
            }
        }

        private void DrawStyledText(InterfaceManager ui, string text, Vector2 position, Color color, TextStyle style, float scale)
        {
            if (string.IsNullOrEmpty(text) || font == null) return;

            try
            {
                var pixelFontSize = GetPixelFontSize();
                if (pixelFontSize == null) return;

                Vector2 drawPosition = new(position.X, position.Y + (GetLineHeight() * scale * 0.5f));

                font.Draw(pixelFontSize.Size, text, drawPosition, Vector2.Zero, Vector2.One * scale, color);

                var textSize = pixelFontSize.Measure(text) * scale;
                var textBaseline = textSize.Y * 0.5f;

                switch (style)
                {
                    case TextStyle.Underline:
                        DrawUnderline(ui, position, textSize, textBaseline, color);
                        break;
                    case TextStyle.Strikethrough:
                        DrawStrikethrough(ui, position, textSize, textBaseline, color);
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, "Celestial League", $"Failed to draw styled text: {ex.Message}");
            }
        }

        private void DrawUnderline(InterfaceManager ui, Vector2 position, Vector2 textSize, float baseline, Color color)
        {
            Rectangle underlineRect = new(
                (int)position.X,
                (int)(position.Y + baseline + 2),
                (int)textSize.X,
                1
            );
            ui.DrawRectangle(underlineRect, color);
        }

        private void DrawStrikethrough(InterfaceManager ui, Vector2 position, Vector2 textSize, float baseline, Color color)
        {
            Rectangle strikeRect = new(
                (int)position.X,
                (int)(position.Y + baseline - textSize.Y * 0.35f),
                (int)textSize.X,
                1
            );
            ui.DrawRectangle(strikeRect, color);
        }

        #endregion

        #region Text Rendering Methods
        private void RenderSingleLineText(InterfaceManager ui, Rectangle contentBounds, Color renderColor)
        {
            Vector2 textSize = MeasureText(text) * TextScale;
            Vector2 position = CalculateTextRenderPosition(contentBounds, textSize);
            DrawText(ui, text, position, renderColor);
        }

        private void RenderWrappedText(InterfaceManager ui, Rectangle contentBounds, Color renderColor)
        {
            var lines = WrapText(text, (int)(contentBounds.Width / TextScale));
            float lineHeight = GetLineHeight() * LineSpacing * TextScale;
            float totalTextHeight = lines.Length * lineHeight;

            float startY = VerticalAlignment switch
            {
                VerticalAlignment.Top => contentBounds.Y,
                VerticalAlignment.Center => contentBounds.Y + (contentBounds.Height - totalTextHeight) * 0.5f,
                VerticalAlignment.Bottom => contentBounds.Y + contentBounds.Height - totalTextHeight,
                _ => contentBounds.Y
            };

            for (int i = 0; i < lines.Length; i++)
            {
                if (string.IsNullOrEmpty(lines[i])) continue;

                Vector2 lineSize = MeasureText(lines[i]) * TextScale;
                Vector2 linePosition = new(
                    CalculateHorizontalAlignment(contentBounds, lineSize.X),
                    startY + (i * lineHeight)
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
                font = Dialog.Languages["english"].Font;

            try
            {
                if (font.Sizes?.Count > 0)
                {
                    cachedPixelFontSize = font.Sizes.First();
                }
            }
            catch (Exception ex)
            {
                Logger.Info("Celestial League", $"Failed to get PixelFont size: {ex.Message}");
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
                Logger.Log(LogLevel.Warn, "Celestial League", $"Failed to measure text: {ex.Message}");
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
                Logger.Log(LogLevel.Warn, "Celestial League", $"Failed to get line height: {ex.Message}");
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
                    Vector2 drawPosition = new(position.X, position.Y);
                    font.Draw(pixelFontSize.Size, text, drawPosition, Vector2.Zero, Vector2.One * TextScale, color);
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, "Celestial League", $"Failed to draw text: {ex.Message}");
            }
        }

        private Vector2 CalculateTextRenderPosition(Rectangle bounds, Vector2 textSize)
        {
            float x = HorizontalAlignment switch
            {
                HorizontalAlignment.Left => bounds.X,
                HorizontalAlignment.Center => bounds.X + (bounds.Width - textSize.X) * 0.5f,
                HorizontalAlignment.Right => bounds.X + bounds.Width - textSize.X,
                _ => bounds.X
            };

            float y = VerticalAlignment switch
            {
                VerticalAlignment.Top => bounds.Y,
                VerticalAlignment.Center => bounds.Y + (bounds.Height - textSize.Y) * 0.5f,
                VerticalAlignment.Bottom => bounds.Y + bounds.Height - textSize.Y,
                _ => bounds.Y
            };

            return new Vector2(x, y);
        }

        private float CalculateHorizontalAlignment(Rectangle bounds, float textWidth)
        {
            return HorizontalAlignment switch
            {
                HorizontalAlignment.Left => bounds.X,
                HorizontalAlignment.Center => bounds.X + (bounds.Width - textWidth) * 0.5f,
                HorizontalAlignment.Right => bounds.X + bounds.Width - textWidth,
                _ => bounds.X
            };
        }

        private string[] WrapText(string text, int maxWidth)
        {
            if (maxWidth <= 0) return [text];

            var lines = new List<string>();
            var words = text.Split(' ');
            var currentLine = "";

            foreach (var word in words)
            {
                var testLine = string.IsNullOrEmpty(currentLine) ? word : currentLine + " " + word;
                var testSize = MeasureText(testLine);

                if (testSize.X * TextScale <= maxWidth)
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

        #endregion

        private void RecalculateSize()
        {
            if (string.IsNullOrEmpty(text) || font == null)
            {
                measuredSize = Vector2.Zero;
                return;
            }

            try
            {
                if (useRichText && segments.Count > 0)
                {
                    if (WordWrap && Layout.Size.X.IsSet)
                    {
                        var parentSize = Parent != null ? LayoutHelper.CalculateSize(Parent) : Vector2.Zero;
                        var contentWidth = Layout.Size.X.Resolve(parentSize.X);
                        var lines = WrapRichText(segments, contentWidth);
                        float totalHeight = lines.Count * GetLineHeight() * LineSpacing * TextScale;
                        measuredSize = new Vector2(contentWidth, totalHeight);
                    }
                    else
                    {
                        float totalWidth = 0f;
                        float maxHeight = 0f;

                        foreach (var segment in segments)
                        {
                            string processedText = ProcessTextStyle(segment.Text, segment.Style);
                            Vector2 segmentSize = MeasureSegmentText(processedText, segment.Style, segment.Scale * TextScale);
                            totalWidth += segmentSize.X;
                            maxHeight = Math.Max(maxHeight, segmentSize.Y);
                        }

                        measuredSize = new Vector2(totalWidth, maxHeight);
                    }
                }
                else
                {
                    if (WordWrap && Layout.Size.X.IsSet)
                    {
                        var parentSize = Parent != null ? LayoutHelper.CalculateSize(Parent) : Vector2.Zero;
                        var contentWidth = Layout.Size.X.Resolve(parentSize.X);
                        var lines = WrapText(text, (int)(contentWidth / TextScale));
                        float totalHeight = lines.Length * GetLineHeight() * LineSpacing * TextScale;
                        measuredSize = new Vector2(contentWidth, totalHeight);
                    }
                    else
                    {
                        measuredSize = MeasureText(text) * TextScale;
                    }
                }

                if (AutoSize && !Layout.Size.X.IsSet && !Layout.Size.Y.IsSet)
                {
                    Layout.AbsoluteSize = measuredSize;
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "Celestial League", $"Failed to recalculate text size: {ex.Message}");
                measuredSize = Vector2.Zero;
            }
        }

        public Vector2 GetTextSize()
        {
            if (useRichText && segmentsDirty)
            {
                ParseRichText();
                segmentsDirty = false;
                sizeDirty = true;
            }

            if (sizeDirty)
            {
                RecalculateSize();
                sizeDirty = false;
            }
            return measuredSize;
        }

        public string GetPlainText()
        {
            if (!useRichText) return text;
            if (segments.Count == 0) return "";
            return string.Join("", segments.Select(s => s.Text));
        }
    }
}
