using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using CelestialLeague.Client.UI.Core;
using Celeste.Mod;

namespace CelestialLeague.Client.UI.Components
{
    public enum StatusType
    {
        Info,
        Success,
        Warning,
        Error,
        Loading
    }

    public class StatusItem
    {
        public string Message { get; set; }
        public StatusType Type { get; set; }
        public DateTime Timestamp { get; set; }
        public float Duration { get; set; } = 5f;
        public bool IsPersistent { get; set; } = false;
        public float Alpha { get; set; } = 1f;
        public float Timer { get; set; } = 0f;

        public StatusItem(string message, StatusType type = StatusType.Info, float duration = 5f)
        {
            Message = message;
            Type = type;
            Duration = duration;
            Timestamp = DateTime.Now;
        }
    }

    public class StatusDisplay : UIComponent
    {
        public SpriteFont Font { get; set; }
        public int MaxItems { get; set; } = 5;
        public float ItemHeight { get; set; } = 30f;
        public float FadeInDuration { get; set; } = 0.3f;
        public float FadeOutDuration { get; set; } = 0.5f;
        public bool ShowTimestamps { get; set; } = false;
        public bool AutoScroll { get; set; } = true;
        public Vector2 ItemPadding { get; set; } = new Vector2(10, 5);

        // color scheme for different status types
        public Dictionary<StatusType, Color> StatusColors { get; private set; }
        public Dictionary<StatusType, Color> BackgroundColors { get; private set; }

        private List<StatusItem> items;
        private float scrollOffset = 0f;
        private bool needsLayout = true;

        // events
        public event Action<StatusItem> ItemAdded;
        public event Action<StatusItem> ItemRemoved;
        public event Action StatusCleared;

        public StatusDisplay(string id = null) : base(id)
        {
            items = new List<StatusItem>();
            CanReceiveInput = true; // for scrolling

            InitializeColors();

            Size = new Vector2(300, 200);
            BackgroundColor = Color.Black * 0.7f;
            BorderColor = Color.Gray;
            BorderWidth = 1;
        }

        private void InitializeColors()
        {
            StatusColors = new Dictionary<StatusType, Color>
            {
                { StatusType.Info, Color.White },
                { StatusType.Success, Color.LightGreen },
                { StatusType.Warning, Color.Yellow },
                { StatusType.Error, Color.Red },
                { StatusType.Loading, Color.Cyan }
            };

            BackgroundColors = new Dictionary<StatusType, Color>
            {
                { StatusType.Info, Color.DarkBlue * 0.3f },
                { StatusType.Success, Color.DarkGreen * 0.3f },
                { StatusType.Warning, Color.DarkOrange * 0.3f },
                { StatusType.Error, Color.DarkRed * 0.3f },
                { StatusType.Loading, Color.DarkCyan * 0.3f }
            };
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();

            UpdateItems();

            if (needsLayout)
            {
                UpdateLayout();
                needsLayout = false;
            }

            if (IsHovered && items.Count > MaxItems)
            {
                HandleScrolling();
            }
        }

        private void UpdateItems()
        {
            var itemsToRemove = new List<StatusItem>();

            foreach (var item in items)
            {
                item.Timer += Engine.DeltaTime;

                if (!item.IsPersistent && item.Timer >= item.Duration)
                {
                    float fadeOutStart = item.Duration - FadeOutDuration;
                    if (item.Timer >= fadeOutStart)
                    {
                        float fadeProgress = (item.Timer - fadeOutStart) / FadeOutDuration;
                        item.Alpha = 1f - fadeProgress;

                        if (item.Alpha <= 0f)
                        {
                            itemsToRemove.Add(item);
                        }
                    }
                }
                else if (item.Timer < FadeInDuration)
                {
                    item.Alpha = item.Timer / FadeInDuration;
                }
                else
                {
                    item.Alpha = 1f;
                }
            }

            foreach (var item in itemsToRemove)
            {
                RemoveItem(item);
            }
        }

        private void UpdateLayout()
        {
            int visibleItems = Math.Min(items.Count, MaxItems);
            Size = new Vector2(Size.X, visibleItems * ItemHeight + ItemPadding.Y * 2);
        }

        private void HandleScrolling()
        {
            var scrollDelta = MInput.Mouse.WheelDelta;
            if (scrollDelta != 0)
            {
                scrollOffset -= scrollDelta * ItemHeight * 0.1f;

                float maxScroll = Math.Max(0, (items.Count - MaxItems) * ItemHeight);
                scrollOffset = MathHelper.Clamp(scrollOffset, 0, maxScroll);
            }
        }

        protected override void OnDraw(SpriteBatch spriteBatch)
        {
            base.OnDraw(spriteBatch);

            if (Font == null || items.Count == 0) return;

            DrawItems(spriteBatch);
        }

        private void DrawItems(SpriteBatch spriteBatch)
        {
            var bounds = AbsoluteBounds;
            int startIndex = (int)(scrollOffset / ItemHeight);
            int endIndex = Math.Min(startIndex + MaxItems, items.Count);

            for (int i = startIndex; i < endIndex; i++)
            {
                var item = items[items.Count - 1 - i];
                float yOffset = (i - startIndex) * ItemHeight;

                var itemBounds = new Rectangle(
                    bounds.X,
                    bounds.Y + (int)yOffset,
                    bounds.Width,
                    (int)ItemHeight
                );

                DrawStatusItem(spriteBatch, item, itemBounds);
            }

            if (items.Count > MaxItems)
            {
                DrawScrollIndicator(spriteBatch, bounds);
            }
        }

        private void DrawStatusItem(SpriteBatch spriteBatch, StatusItem item, Rectangle itemBounds)
        {
            var pixel = GetPixelTexture(spriteBatch.GraphicsDevice);

            var bgColor = BackgroundColors[item.Type] * item.Alpha * Alpha;
            spriteBatch.Draw(pixel, itemBounds, bgColor);

            string displayText = item.Message;
            if (ShowTimestamps)
            {
                displayText = $"[{item.Timestamp:HH:mm:ss}] {displayText}";
            }

            if (item.Type == StatusType.Loading)
            {
                int dots = ((int)(item.Timer * 2) % 4);
                displayText += new string('.', dots);
            }

            var textBounds = new Rectangle(
                itemBounds.X + (int)ItemPadding.X,
                itemBounds.Y + (int)ItemPadding.Y,
                itemBounds.Width - (int)(ItemPadding.X * 2),
                itemBounds.Height - (int)(ItemPadding.Y * 2)
            );

            var textColor = StatusColors[item.Type] * item.Alpha * Alpha;
            DrawWrappedText(spriteBatch, displayText, textBounds, textColor);
        }

        private void DrawWrappedText(SpriteBatch spriteBatch, string text, Rectangle bounds, Color color)
        {
            var words = text.Split(' ');
            var lines = new List<string>();
            var currentLine = "";

            foreach (var word in words)
            {
                var testLine = string.IsNullOrEmpty(currentLine) ? word : currentLine + " " + word;
                var testSize = Font.MeasureString(testLine);

                if (testSize.X > bounds.Width && !string.IsNullOrEmpty(currentLine))
                {
                    lines.Add(currentLine);
                    currentLine = word;
                }
                else
                {
                    currentLine = testLine;
                }
            }

            if (!string.IsNullOrEmpty(currentLine))
            {
                lines.Add(currentLine);
            }

            for (int i = 0; i < lines.Count; i++)
            {
                var linePosition = new Vector2(bounds.X, bounds.Y + i * Font.LineSpacing);
                if (linePosition.Y + Font.LineSpacing > bounds.Bottom) break;

                spriteBatch.DrawString(Font, lines[i], linePosition, color);
            }
        }

        private void DrawScrollIndicator(SpriteBatch spriteBatch, Rectangle bounds)
        {
            var pixel = GetPixelTexture(spriteBatch.GraphicsDevice);

            var trackBounds = new Rectangle(bounds.Right - 8, bounds.Y, 6, bounds.Height);
            spriteBatch.Draw(pixel, trackBounds, Color.DarkGray * 0.5f * Alpha);

            float thumbHeight = Math.Max(20, bounds.Height * MaxItems / (float)items.Count);
            float thumbY = bounds.Y + (scrollOffset / ((items.Count - MaxItems) * ItemHeight)) * (bounds.Height - thumbHeight);

            var thumbBounds = new Rectangle(bounds.Right - 7, (int)thumbY, 4, (int)thumbHeight);
            spriteBatch.Draw(pixel, thumbBounds, Color.Gray * Alpha);
        }

        private static Texture2D pixelTexture;
        private new static Texture2D GetPixelTexture(GraphicsDevice graphicsDevice)
        {
            if (pixelTexture == null)
            {
                pixelTexture = new Texture2D(graphicsDevice, 1, 1);
                pixelTexture.SetData(new[] { Color.White });
            }
            return pixelTexture;
        }

        public void AddStatus(string message, StatusType type = StatusType.Info, float duration = 5f, bool persistent = false)
        {
            var item = new StatusItem(message, type, duration)
            {
                IsPersistent = persistent
            };

            items.Add(item);

            while (items.Count > MaxItems * 2) // keep some buffer for smooth scrolling
            {
                var oldestItem = items[0];
                items.RemoveAt(0);
                ItemRemoved?.Invoke(oldestItem);
            }

            if (AutoScroll)
            {
                ScrollToBottom();
            }

            needsLayout = true;
            ItemAdded?.Invoke(item);

            Logger.Log(LogLevel.Verbose, "CelestialLeague", $"Status added: [{type}] {message}");
        }

        public void RemoveItem(StatusItem item)
        {
            if (items.Remove(item))
            {
                needsLayout = true;
                ItemRemoved?.Invoke(item);
            }
        }

        public void ClearAll()
        {
            items.Clear();
            scrollOffset = 0f;
            needsLayout = true;
            StatusCleared?.Invoke();

            Logger.Log(LogLevel.Verbose, "CelestialLeague", "Status display cleared");
        }

        public void ClearType(StatusType type)
        {
            var itemsToRemove = items.Where(i => i.Type == type).ToList();
            foreach (var item in itemsToRemove)
            {
                RemoveItem(item);
            }
        }

        public void ScrollToTop()
        {
            scrollOffset = 0f;
        }

        public void ScrollToBottom()
        {
            float maxScroll = Math.Max(0, (items.Count - MaxItems) * ItemHeight);
            scrollOffset = maxScroll;
        }

        public int GetItemCount(StatusType? type = null)
        {
            if (type.HasValue)
            {
                return items.Count(i => i.Type == type.Value);
            }
            return items.Count;
        }

        public bool HasErrors()
        {
            return items.Any(i => i.Type == StatusType.Error);
        }

        public bool HasWarnings()
        {
            return items.Any(i => i.Type == StatusType.Warning);
        }

        // convenience methods
        public void ShowInfo(string message, float duration = 5f) => AddStatus(message, StatusType.Info, duration);
        public void ShowSuccess(string message, float duration = 3f) => AddStatus(message, StatusType.Success, duration);
        public void ShowWarning(string message, float duration = 7f) => AddStatus(message, StatusType.Warning, duration);
        public void ShowError(string message, float duration = 10f) => AddStatus(message, StatusType.Error, duration);
        public void ShowLoading(string message) => AddStatus(message, StatusType.Loading, 0f, true);

        public override void Cleanup()
        {
            ClearAll();
            ItemAdded = null;
            ItemRemoved = null;
            StatusCleared = null;
            base.Cleanup();
        }
    }
}