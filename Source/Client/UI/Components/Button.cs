using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Monocle;
using System;
using CelestialLeague.Client.UI.Core;

namespace CelestialLeague.Client.UI.Components
{
    public class Button : UIComponent
    {
        public string Text { get; set; }
        public SpriteFont Font { get; set; }
        public Color TextColor { get; set; } = Color.White;
        public Color HoverTextColor { get; set; } = Color.Yellow;
        public Color DisabledTextColor { get; set; } = Color.Gray;
        public Color NormalBackgroundColor { get; set; } = Color.DarkBlue;
        public Color HoverBackgroundColor { get; set; } = Color.Blue;
        public Color PressedBackgroundColor { get; set; } = Color.Navy;
        public Color DisabledBackgroundColor { get; set; } = Color.DarkGray;

        // events
        public event Action<Button> ButtonClicked;
        public event Action<Button> ButtonPressed;
        public event Action<Button> ButtonReleased;

        // statae
        private bool wasPressed = false;

        public Button(string text = "", string id = null) : base(id)
        {
            Text = text ?? "";
            CanReceiveInput = true;
            IsFocusable = true;
            
            BorderWidth = 2;
            BorderColor = Color.White;
            BackgroundColor = NormalBackgroundColor;
            
            if (string.IsNullOrEmpty(Text) == false)
            {
                AutoSizeToText();
            }
        }

        public void AutoSizeToText()
        {
            if (Font != null && !string.IsNullOrEmpty(Text))
            {
                var textSize = Font.MeasureString(Text);
                Size = new Vector2(textSize.X + 20, textSize.Y + 10);
            }
            else
            {
                Size = new Vector2(100, 30);
            }
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();
            
            if (!IsEnabled)
            {
                BackgroundColor = DisabledBackgroundColor;
            }
            else if (IsPressed)
            {
                BackgroundColor = PressedBackgroundColor;
            }
            else if (IsHovered)
            {
                BackgroundColor = HoverBackgroundColor;
            }
            else
            {
                BackgroundColor = NormalBackgroundColor;
            }

            if (IsPressed && !wasPressed)
            {
                ButtonPressed?.Invoke(this);
            }
            else if (!IsPressed && wasPressed)
            {
                ButtonReleased?.Invoke(this);
            }
            
            wasPressed = IsPressed;
        }

        protected override void OnDraw(SpriteBatch spriteBatch)
        {
            base.OnDraw(spriteBatch);
            
            if (Font != null && !string.IsNullOrEmpty(Text))
            {
                DrawText(spriteBatch);
            }
        }

        private void DrawText(SpriteBatch spriteBatch)
        {
            var bounds = AbsoluteBounds;
            var textSize = Font.MeasureString(Text);
            
            var textPosition = new Vector2(
                bounds.X + (bounds.Width - textSize.X) / 2,
                bounds.Y + (bounds.Height - textSize.Y) / 2
            );

            Color currentTextColor;
            if (!IsEnabled)
                currentTextColor = DisabledTextColor;
            else if (IsHovered)
                currentTextColor = HoverTextColor;
            else
                currentTextColor = TextColor;

            spriteBatch.DrawString(Font, Text, textPosition, currentTextColor * Alpha);
        }

        protected override void OnMouseClickInternal(Vector2 mousePosition)
        {
            base.OnMouseClickInternal(mousePosition);
            
            if (IsEnabled)
            {
                ButtonClicked?.Invoke(this);
            }
        }

        protected override void OnKeyPressedInternal(Keys key)
        {
            base.OnKeyPressedInternal(key);
            
            if (IsEnabled && IsFocused && (key == Keys.Enter || key == Keys.Space))
            {
                ButtonClicked?.Invoke(this);
            }
        }

        // utility
        public void SetText(string text)
        {
            Text = text ?? "";
            AutoSizeToText();
        }

        public void SetFont(SpriteFont font)
        {
            Font = font;
            AutoSizeToText();
        }

        public void SetColors(Color normal, Color hover, Color pressed, Color disabled)
        {
            NormalBackgroundColor = normal;
            HoverBackgroundColor = hover;
            PressedBackgroundColor = pressed;
            DisabledBackgroundColor = disabled;
        }

        public void SetTextColors(Color normal, Color hover, Color disabled)
        {
            TextColor = normal;
            HoverTextColor = hover;
            DisabledTextColor = disabled;
        }
    }
}