using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace CelestialLeague.Client.UI.Core
{
    public abstract class UIComponent
    {
        public string Id { get; set; }
        public Vector2 Position { get; set; }
        public Vector2 Size { get; set; }
        public bool IsVisible { get; set; } = true;
        public bool IsEnabled { get; set; } = true;
        public bool CanReceiveInput { get; set; } = true;
        public bool IsFocusable { get; set; } = false;
        public Color BackgroundColor { get; set; } = Color.Transparent;
        public Color BorderColor { get; set; } = Color.Transparent;
        public int BorderWidth { get; set; } = 0;
        public float Alpha { get; set; } = 1.0f;

        // hierarchy
        public UIComponent Parent { get; set; }
        protected List<UIComponent> children;

        // state
        public bool IsHovered { get; protected set; }
        public bool IsFocused { get; protected set; }
        public bool IsPressed { get; protected set; }

        // events
        public event Action<UIComponent> OnClick;
        public event Action<UIComponent> OnHover;
        public event Action<UIComponent> OnFocus;
        public event Action<UIComponent> OnBlur;

        // static pixel texture cache
        private static Texture2D pixelTexture;
        private static GraphicsDevice lastGraphicsDevice;

        public UIComponent()
        {
            children = new List<UIComponent>();
            Id = Guid.NewGuid().ToString();
        }

		public UIComponent(string id) : this()
		{
			Id = id;
		}

        public virtual void AddChild(UIComponent child)
        {
            if (child == null || children.Contains(child)) return;

            child.Parent?.RemoveChild(child);
            child.Parent = this;
            children.Add(child);
            child.OnAdded();
        }

        public virtual void RemoveChild(UIComponent child)
        {
            if (child == null || !children.Contains(child)) return;

            child.Parent = null;
            children.Remove(child);
            child.OnRemoved();
        }

        public virtual void RemoveAllChildren()
        {
            var childrenCopy = new List<UIComponent>(children);
            foreach (var child in childrenCopy)
            {
                RemoveChild(child);
            }
        }

        public UIComponent FindChild(string id)
        {
            foreach (var child in children)
            {
                if (child.Id == id) return child;
                
                var found = child.FindChild(id);
                if (found != null) return found;
            }
            return null;
        }

        public Rectangle Bounds => new Rectangle((int)Position.X, (int)Position.Y, (int)Size.X, (int)Size.Y);

        public Vector2 AbsolutePosition
        {
            get
            {
                if (Parent == null) return Position;
                return Parent.AbsolutePosition + Position;
            }
        }

        public Rectangle AbsoluteBounds
        {
            get
            {
                var absPos = AbsolutePosition;
                return new Rectangle((int)absPos.X, (int)absPos.Y, (int)Size.X, (int)Size.Y);
            }
        }

        public virtual bool ContainsPoint(Vector2 point)
        {
            return AbsoluteBounds.Contains((int)point.Y, (int)point.X);
        }

        public virtual UIComponent GetComponentAt(Vector2 point)
        {
            if (!IsVisible || !ContainsPoint(point)) return null;

            // check children in reverse order for top-to-bottom
            for (int i = children.Count - 1; i >= 0; i--)
            {
                var child = children[i].GetComponentAt(point);
                if (child != null) return child;
            }

            return CanReceiveInput ? this : null;
        }

        public virtual void Update()
        {
            if (!IsVisible) return;

            OnUpdate();

            foreach (var child in children)
            {
                child.Update();
            }
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            if (!IsVisible) return;

            DrawBackground(spriteBatch);
            OnDraw(spriteBatch);
            DrawBorder(spriteBatch);

            // Draw children
            foreach (var child in children)
            {
                child.Draw(spriteBatch);
            }
        }

        protected virtual void DrawBackground(SpriteBatch spriteBatch)
        {
            if (BackgroundColor != Color.Transparent)
            {
                var bounds = AbsoluteBounds;
                var pixel = GetPixelTexture(spriteBatch.GraphicsDevice);
                spriteBatch.Draw(pixel, bounds, BackgroundColor * Alpha);
            }
        }

        protected virtual void DrawBorder(SpriteBatch spriteBatch)
        {
            if (BorderWidth > 0 && BorderColor != Color.Transparent)
            {
                var bounds = AbsoluteBounds;
                var pixel = GetPixelTexture(spriteBatch.GraphicsDevice);
                var color = BorderColor * Alpha;

                // Draw border rectangles
                spriteBatch.Draw(pixel, new Rectangle(bounds.X, bounds.Y, bounds.Width, BorderWidth), color); // Top
                spriteBatch.Draw(pixel, new Rectangle(bounds.X, bounds.Bottom - BorderWidth, bounds.Width, BorderWidth), color); // Bottom
                spriteBatch.Draw(pixel, new Rectangle(bounds.X, bounds.Y, BorderWidth, bounds.Height), color); // Left
                spriteBatch.Draw(pixel, new Rectangle(bounds.Right - BorderWidth, bounds.Y, BorderWidth, bounds.Height), color); // Right
            }
        }

        private static Texture2D GetPixelTexture(GraphicsDevice graphicsDevice)
        {
            // Recreate if graphics device changed or texture doesn't exist
            if (pixelTexture == null || lastGraphicsDevice != graphicsDevice)
            {
                pixelTexture?.Dispose(); // Clean up old texture if it exists
                pixelTexture = new Texture2D(graphicsDevice, 1, 1);
                pixelTexture.SetData(new[] { Color.White });
                lastGraphicsDevice = graphicsDevice;
            }
            return pixelTexture;
        }

        public static void DisposeStaticResources()
        {
            pixelTexture?.Dispose();
            pixelTexture = null;
            lastGraphicsDevice = null;
        }

        public virtual void OnMouseEnter()
        {
            IsHovered = true;
            OnHover?.Invoke(this);
            OnMouseEnterInternal();
        }

        public virtual void OnMouseLeave()
        {
            IsHovered = false;
            IsPressed = false;
            OnMouseLeaveInternal();
        }

        public virtual void OnMouseClick(Vector2 mousePosition)
        {
            if (!IsEnabled || !CanReceiveInput) return;

            IsPressed = true;
            OnClick?.Invoke(this);
            OnMouseClickInternal(mousePosition);
        }

        public virtual void OnGainedFocus()
        {
            IsFocused = true;
            OnFocus?.Invoke(this);
            OnGainedFocusInternal();
        }

        public virtual void OnLostFocus()
        {
            IsFocused = false;
            OnBlur?.Invoke(this);
            OnLostFocusInternal();
        }

        public virtual void OnKeyPressed(Keys key)
        {
            OnKeyPressedInternal(key);
        }

        public virtual void HandleKeyboardInput()
        {
            // override in derived classes for specific keyboard handling
        }

        protected virtual void OnUpdate() { }
        protected virtual void OnDraw(SpriteBatch spriteBatch) { }
        protected virtual void OnAdded() { }
        protected virtual void OnRemoved() { }
        public virtual void OnShown() { }
        public virtual void OnHidden() { }
        protected virtual void OnMouseEnterInternal() { }
        protected virtual void OnMouseLeaveInternal() { }
        protected virtual void OnMouseClickInternal(Vector2 mousePosition) { }
        protected virtual void OnGainedFocusInternal() { }
        protected virtual void OnLostFocusInternal() { }
        protected virtual void OnKeyPressedInternal(Keys key) { }

        public void SetPosition(float x, float y)
        {
            Position = new Vector2(x, y);
        }

        public void SetSize(float width, float height)
        {
            Size = new Vector2(width, height);
        }

        public void Center(Vector2 containerSize)
        {
            Position = new Vector2(
                (containerSize.X - Size.X) / 2,
                (containerSize.Y - Size.Y) / 2
            );
        }

        public void Show()
        {
            IsVisible = true;
            OnShown();
        }

        public void Hide()
        {
            IsVisible = false;
            OnHidden();
        }

        public void Enable()
        {
            IsEnabled = true;
        }

        public void Disable()
        {
            IsEnabled = false;
        }

        public virtual void Cleanup()
        {
            OnClick = null;
            OnHover = null;
            OnFocus = null;
            OnBlur = null;
            
            RemoveAllChildren();
            Parent?.RemoveChild(this);
        }
    }
}