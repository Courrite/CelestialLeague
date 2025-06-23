using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace CelestialLeague.Client.UI.Core
{
    public abstract class UIComponent
    {
        // static texture for drawing rectangles - shared across all components to save memory
        private static Texture2D pixelTexture;

        // hierarchy
        public UIComponent Parent { get; set; }
        public List<UIComponent> Children { get; private set; }

        // identity
        public string Id { get; set; }
        public object Tag { get; set; }

        // transform - position relative to parent, size in pixels
        public Vector2 Position { get; set; }
        public Vector2 Size { get; set; }

        // bounds calculations
        public Rectangle Bounds => new Rectangle((int)Position.X, (int)Position.Y, (int)Size.X, (int)Size.Y);
        public Rectangle AbsoluteBounds => new Rectangle((int)AbsolutePosition.X, (int)AbsolutePosition.Y, (int)Size.X, (int)Size.Y);
        public Vector2 AbsolutePosition => Parent != null ? Parent.AbsolutePosition + Position : Position;

        // visual properties
        public bool IsVisible { get; set; } = true;
        public float Alpha { get; set; } = 1f;
        public Color BackgroundColor { get; set; } = Color.Transparent;
        public Color BorderColor { get; set; } = Color.Transparent;
        public int BorderWidth { get; set; } = 0;

        // interaction states
        public bool CanReceiveInput { get; set; } = false;
        public bool IsFocusable { get; set; } = false;
        public bool IsEnabled { get; set; } = true;
        public bool IsHovered { get; set; } = false;
        public bool IsPressed { get; set; } = false;
        public bool IsFocused { get; set; } = false;

        public UIComponent(string id = null)
        {
            Id = id;
            Children = [];
        }

        // hierarhcy management
        public void AddChild(UIComponent child)
        {
            if (child == null) return;

            child.RemoveFromParent();

            Children.Add(child);
            child.Parent = this;
        }

        public void RemoveChild(UIComponent child)
        {
            if (child == null) return;

            if (Children.Remove(child))
            {
                child.Parent = null;
            }
        }

        public void RemoveFromParent()
        {
            Parent?.RemoveChild(this);
        }

        // finding components
        public T FindChild<T>(string id) where T : UIComponent
        {
            return FindChild(id) as T;
        }

        public UIComponent FindChild(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;

            foreach (var child in Children)
            {
                if (child.Id == id) return child;
            }

            foreach (var child in Children)
            {
                var found = child.FindChild(id);
                if (found != null) return found;
            }

            return null;
        }

        // update cycle
        public virtual void Update()
        {
            if (!IsVisible) return;

            OnUpdate();

            for (int i = Children.Count - 1; i >= 0; i--)
            {
                if (i < Children.Count)
                {
                    Children[i].Update();
                }
            }
        }

        protected virtual void OnUpdate()
        {
            // override in derived classes
            // base implementation is empty cuz not all components need update logic
        }

        // render cycle
        public void Render(SpriteBatch spriteBatch)
        {
            if (!IsVisible) return;

            OnRender(spriteBatch);

            foreach (var child in Children)
            {
                child.Render(spriteBatch);
            }
        }

        protected virtual void OnRender(SpriteBatch spriteBatch)
        {
            OnDraw(spriteBatch);

            // override in derived classes
        }

        protected virtual void OnDraw(SpriteBatch spriteBatch)
        {
            if (BackgroundColor != Color.Transparent)
            {
                DrawBackground(spriteBatch);
            }

            if (BorderWidth > 0 && BorderColor != Color.Transparent)
            {
                DrawBorder(spriteBatch);
            }
        }

        // drwaing helpers
        protected virtual void DrawBackground(SpriteBatch spriteBatch)
        {
            var pixel = GetPixelTexture(spriteBatch.GraphicsDevice);
            var bounds = AbsoluteBounds;
            spriteBatch.Draw(pixel, bounds, BackgroundColor * Alpha);
        }

        protected virtual void DrawBorder(SpriteBatch spriteBatch)
        {
            var pixel = GetPixelTexture(spriteBatch.GraphicsDevice);
            var bounds = AbsoluteBounds;
            var borderColor = BorderColor * Alpha;

            // T (top)
            spriteBatch.Draw(pixel, new Rectangle(bounds.X, bounds.Y, bounds.Width, BorderWidth), borderColor);
            // B (bottmo)
            spriteBatch.Draw(pixel, new Rectangle(bounds.X, bounds.Bottom - BorderWidth, bounds.Width, BorderWidth), borderColor);
            // L (left)
            spriteBatch.Draw(pixel, new Rectangle(bounds.X, bounds.Y, BorderWidth, bounds.Height), borderColor);
            // R (right)
            spriteBatch.Draw(pixel, new Rectangle(bounds.Right - BorderWidth, bounds.Y, BorderWidth, bounds.Height), borderColor);
        }

        protected static Texture2D GetPixelTexture(GraphicsDevice graphicsDevice)
        {
            if (pixelTexture == null)
            {
                pixelTexture = new Texture2D(graphicsDevice, 1, 1);
                pixelTexture.SetData(new[] { Color.White });
            }
            return pixelTexture;
        }

        // mouse events
        public virtual void OnMouseEnter()
        {
            IsHovered = true;
        }

        public virtual void OnMouseLeave()
        {
            IsHovered = false;
        }

        public virtual void OnMouseMove(Vector2 mousePosition)
        {
            // most components don't need mouse move, but sliders/drag operations do
        }

        public virtual void OnMouseDown(Vector2 mousePosition)
        {
            if (!CanReceiveInput || !IsEnabled) return;

            IsPressed = true;

            // request focus on mouse down, not click, for better UX
            if (IsFocusable)
            {
                RequestFocus();
            }
        }

        public virtual void OnMouseUp(Vector2 mousePosition)
        {
            if (!CanReceiveInput || !IsEnabled) return;

            IsPressed = false;
            // reset press state regardless of where mouse up occurs
        }

        public virtual void OnClick(Vector2 mousePosition)
        {
            if (!CanReceiveInput || !IsEnabled) return;

            // click is mouse down + up on same component
            // override in derived classes
        }

        // keyboard events
        public virtual void OnKeyPressed(Keys key)
        {
            if (!CanReceiveInput || !IsEnabled || !IsFocused) return;

            switch (key)
            {
                case Keys.Tab:
                    MoveFocusToNext();
                    break;
                case Keys.Enter:
                case Keys.Space:
                    // enter/space activates component like clicking
                    OnClick(Vector2.Zero);
                    break;
            }
        }

        public virtual void OnKeyReleased(Keys key)
        {
            if (!CanReceiveInput || !IsEnabled || !IsFocused) return;

            // most components don't need key release events
        }

        public virtual void OnTextInput(char character)
        {
            if (!CanReceiveInput || !IsEnabled || !IsFocused) return;

            // only text input components need this - textboxes, etc.
        }

        // focuz events
        public virtual void OnGainedFocus()
        {
            IsFocused = true;
            // override in derived classes
        }

        public virtual void OnLostFocus()
        {
            IsFocused = false;
            // override in derived classes
        }

        // hit testing
        public virtual bool ContainsPoint(Point point)
        {
            return AbsoluteBounds.Contains(point);
        }

        public virtual UIComponent GetComponentAt(Point point)
        {
            if (!IsVisible || !ContainsPoint(point)) return null;

            // check children first cuz they render on top
            for (int i = Children.Count - 1; i >= 0; i--)
            {
                var child = Children[i].GetComponentAt(point);
                if (child != null) return child;
            }

            return CanReceiveInput ? this : null;
        }

        // util methods
        public void SetPosition(float x, float y)
        {
            Position = new Vector2(x, y);
        }

        public void SetSize(float width, float height)
        {
            Size = new Vector2(width, height);
        }

        public void SetBounds(Rectangle bounds)
        {
            Position = new Vector2(bounds.X, bounds.Y);
            Size = new Vector2(bounds.Width, bounds.Height);
        }

        // focus management
        protected virtual void RequestFocus()
        {
            UIManager.Instance?.SetFocusedComponent(this);
        }

        protected virtual void MoveFocusToNext()
        {
            UIManager.Instance?.MoveFocusToNext();
        }

        public virtual void Cleanup()
        {
            RemoveFromParent();

            foreach (var child in Children)
            {
                child.Cleanup();
            }
            Children.Clear();
        }

        // debug helper
        public override string ToString()
        {
            return $"{GetType().Name}({Id ?? "unnamed"}) at {Position} size {Size}";
        }
    }
}
