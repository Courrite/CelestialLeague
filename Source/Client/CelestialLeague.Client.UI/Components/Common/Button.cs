using CelestialLeague.Client.UI;
using IL.Monocle;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace CelestialLeague.Client.UI.Components
{
    public class Button : Panel
    {
        public bool AutoColor { get; set; } = true;

        public new Color BackgroundColor
        {
            get => base.BackgroundColor;
            set
            {
                base.BackgroundColor = value;
                if (AutoColor)
                    ApplyAutoColors(value);
            }
        }

        public Color NormalColor { get; set; } = Color.DarkGray;
        public Color HoverColor { get; set; } = Color.Gray;
        public Color PressedColor { get; set; } = Color.LightGray;
        public Color DisabledColor { get; set; } = Color.DarkSlateGray;

        private ButtonComponentState currentState = ButtonComponentState.Normal;

        public Button()
        {
            CanReceiveFocus = true;
            base.BackgroundColor = NormalColor; // ensure base initial value
            PanelBorders = new Borders(1);

            // wire up events
            OnMouseEnter += (component) => UpdateButtonState(ButtonComponentState.Hover);
            OnMouseExit += (component) => UpdateButtonState(ButtonComponentState.Normal);
            OnPressed += (component) => UpdateButtonState(ButtonComponentState.Pressed);
            OnReleased += (component) => UpdateButtonState(IsEnabled && ContainsPoint(new Vector2(Mouse.GetState().X, Mouse.GetState().Y)) ? ButtonComponentState.Hover : ButtonComponentState.Normal);
            OnFocusGained += (component) => UpdateButtonState(ButtonComponentState.Hover);
            OnFocusLost += (component) => UpdateButtonState(ButtonComponentState.Normal);
        }

        protected override void UpdateSelf(InterfaceManager ui)
        {
            base.UpdateSelf(ui);

            // update state based on enabled status
            if (!IsEnabled && currentState != ButtonComponentState.Disabled)
            {
                UpdateButtonState(ButtonComponentState.Disabled);
            }
        }

        protected override void RenderSelf(InterfaceManager ui)
        {
            base.RenderSelf(ui);
        }

        private void UpdateButtonState(ButtonComponentState newState)
        {
            if (currentState == newState) return;

            currentState = newState;

            BackgroundColor = currentState switch
            {
                ButtonComponentState.Normal => NormalColor,
                ButtonComponentState.Hover => HoverColor,
                ButtonComponentState.Pressed => PressedColor,
                ButtonComponentState.Disabled => DisabledColor,
                _ => NormalColor
            };
        }

        private void ApplyAutoColors(Color baseColor)
        {
            // Simple heuristic for state colors; tweak multipliers to taste
            NormalColor = baseColor;
            HoverColor = Adjust(baseColor, 1.08f);   // slightly lighter
            PressedColor = Adjust(baseColor, 0.92f); // slightly darker
            DisabledColor = new Color(
                (int)(baseColor.R * 0.6f),
                (int)(baseColor.G * 0.6f),
                (int)(baseColor.B * 0.6f),
                baseColor.A
            );
        }

        private static Color Adjust(Color c, float factor)
        {
            return new Color(
                (int)MathHelper.Clamp(c.R * factor, 0, 255),
                (int)MathHelper.Clamp(c.G * factor, 0, 255),
                (int)MathHelper.Clamp(c.B * factor, 0, 255),
                c.A
            );
        }
    }

    public enum ButtonComponentState
    {
        Normal,
        Hover,
        Pressed,
        Disabled
    }
}
