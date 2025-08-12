using Celeste;
using Celeste.Mod;
using CelestialLeague.Client.UI.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Monocle;
using System;
using System.IO;

namespace CelestialLeague.Client.UI.Core
{
    public class InterfaceManager : Entity
    {
        private static InterfaceManager instance;
        public static InterfaceManager Instance => instance;

        private IUIComponent rootContainer;
        private IUIComponent focusedComponent;

        // input states
        public MouseState CurrentMouseState { get; private set; }
        public MouseState PreviousMouseState { get; private set; }
        public KeyboardState CurrentKeyboardState { get; private set; }
        public KeyboardState PreviousKeyboardState { get; private set; }

        // resources
        public SpriteBatch SpriteBatch { get; private set; }
        public GraphicsDevice GraphicsDevice { get; private set; }
        public Texture2D PixelTexture { get; private set; }

        // ui state
        public bool IsVisible { get; set; }
        public Vector2 MousePosition => new Vector2(CurrentMouseState.X, CurrentMouseState.Y);

        // properties
        public int ScreenWidth => Engine.Width;
        public int ScreenHeight => Engine.Height;
        public Rectangle ScreenBounds => new Rectangle(0, 0, ScreenWidth, ScreenHeight);

        private bool wasMouseVisible;

        public InterfaceManager() : base(Vector2.Zero)
        {
            Tag = Tags.HUD | Tags.Global;
            Depth = -100000;
            instance = this;
            IsVisible = true;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);

            FontLoader.LoadFonts();
            LogAvailableFonts();

            wasMouseVisible = Engine.Instance.IsMouseVisible;
            MInput.Active = true;
            MInput.Disabled = false;
            Engine.Instance.IsMouseVisible = true;

            GraphicsDevice = Engine.Graphics.GraphicsDevice;
            SpriteBatch = new SpriteBatch(GraphicsDevice);

            PixelTexture = new Texture2D(GraphicsDevice, 1, 1);
            PixelTexture.SetData(new[] { Color.White });

            CreateRootContainer();

            Panel panel = new Panel();
            panel.Layout.RelativePosition = Vector2.Zero;
            AddChild(panel);

            Components.Text text = new Components.Text();
            text.Font = Fonts.Get("Montserrat Regular");
            text.Content = "cl_test_text".DialogClean();
            text.Layout.Anchor = Anchor.MiddleCenter;
            text.Layout.RelativePosition = Vector2.Zero;
            text.BackgroundTransparency = 0.5f;
            AddChild(text);
        }

        private void LogAvailableFonts()
        {
            Logger.Info("CelestialLeague", "=== Available Font Paths ===");

            if (Fonts.paths != null)
            {
                foreach (var fontPath in Fonts.paths.Keys)
                {
                    Logger.Info("CelestialLeague", $"Available font path: {fontPath}");
                }
            }
            else
            {
                Logger.Warn("CelestialLeague", "Fonts.paths is null");
            }
        }

        private void CreateRootContainer()
        {
            rootContainer = new Panel
            {
                Name = "RootContainer",
                BackgroundTransparency = 1,
            };

            rootContainer.Layout.AbsoluteSize = new Vector2(ScreenWidth, ScreenHeight);
            rootContainer.Layout.AbsolutePosition = Vector2.Zero;
            rootContainer.Parent = null;

            Logger.Log(LogLevel.Info, "Celestial League", $"Root container created with size {ScreenWidth}x{ScreenHeight}");
        }

        public override void Removed(Scene scene)
        {
            base.Removed(scene);

            Engine.Instance.IsMouseVisible = wasMouseVisible;

            PixelTexture?.Dispose();
            SpriteBatch?.Dispose();

            if (instance == this)
                instance = null;

            Logger.Log(LogLevel.Info, "Celestial League", "InterfaceManager removed from scene");
        }

        // component management
        public void AddChild(IUIComponent child)
        {
            if (rootContainer != null)
            {
                Logger.Log(LogLevel.Info, "Celestial League", $"Adding child: {child.GetType().Name}");
                rootContainer.AddChild(child);
            }
            else
            {
                Logger.Log(LogLevel.Warn, "Celestial League", "Cannot add child - root container is null");
            }
        }

        public void RemoveChild(IUIComponent child) => rootContainer?.RemoveChild(child);
        public void ClearChildren() => rootContainer?.ClearChildren();
        public T FindChild<T>(string name = null) where T : class, IUIComponent => rootContainer?.FindChild<T>(name);

        public override void Update()
        {
            base.Update();
            UpdateInput();

            if (IsVisible && rootContainer != null)
            {
                rootContainer.Layout.AbsoluteSize = new Vector2(ScreenWidth, ScreenHeight);

                rootContainer.Update(this);
                HandleFocus();
            }
        }

        private void HandleFocus()
        {
            if (IsLeftMousePressed())
            {
                var clickedComponent = FindComponentAt(MousePosition);
                SetFocus(clickedComponent);
            }
        }

        private IUIComponent FindComponentAt(Vector2 position)
        {
            return FindComponentAtRecursive(rootContainer, position);
        }

        private IUIComponent FindComponentAtRecursive(IUIComponent component, Vector2 position)
        {
            if (!component.IsVisible || !component.ContainsPoint(position))
                return null;

            foreach (var child in component.Children)
            {
                var found = FindComponentAtRecursive(child, position);
                if (found != null && found.CanReceiveFocus)
                    return found;
            }

            return component.CanReceiveFocus ? component : null;
        }

        public void SetFocus(IUIComponent component)
        {
            if (focusedComponent == component) return;

            if (focusedComponent != null)
            {
                focusedComponent.FocusLost();
            }

            focusedComponent = component;

            if (focusedComponent != null)
            {
                focusedComponent.FocusGained();
            }
        }

        public IUIComponent GetFocusedComponent() => focusedComponent;

        private void UpdateInput()
        {
            PreviousMouseState = CurrentMouseState;
            CurrentMouseState = Mouse.GetState();

            PreviousKeyboardState = CurrentKeyboardState;
            CurrentKeyboardState = Keyboard.GetState();
        }

        public override void Render()
        {
            base.Render();
            if (!IsVisible || rootContainer == null) return;

            SpriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                SamplerState.PointClamp,
                DepthStencilState.None,
                RasterizerState.CullNone,
                null,
                Matrix.Identity
            );

            rootContainer.Render(this);

            SpriteBatch.End();
        }

        // Input helper methods
        public bool IsLeftMousePressed()
        {
            return CurrentMouseState.LeftButton == ButtonState.Pressed && PreviousMouseState.LeftButton == ButtonState.Released;
        }

        public bool IsLeftMouseDown()
        {
            return CurrentMouseState.LeftButton == ButtonState.Pressed;
        }

        public bool IsLeftMouseReleased()
        {
            return CurrentMouseState.LeftButton == ButtonState.Released && PreviousMouseState.LeftButton == ButtonState.Pressed;
        }

        public bool IsRightMousePressed()
        {
            return CurrentMouseState.RightButton == ButtonState.Pressed && PreviousMouseState.RightButton == ButtonState.Released;
        }

        public bool IsRightMouseDown()
        {
            return CurrentMouseState.RightButton == ButtonState.Pressed;
        }

        public bool IsRightMouseReleased()
        {
            return CurrentMouseState.RightButton == ButtonState.Released && PreviousMouseState.RightButton == ButtonState.Pressed;
        }

        public bool IsKeyPressed(Keys key)
        {
            return CurrentKeyboardState.IsKeyDown(key) && !PreviousKeyboardState.IsKeyDown(key);
        }

        public bool IsKeyDown(Keys key)
        {
            return CurrentKeyboardState.IsKeyDown(key);
        }

        public bool IsMouseButtonPressed(MouseButton button)
        {
            return button switch
            {
                MouseButton.Left => IsLeftMousePressed(),
                MouseButton.Right => IsRightMousePressed(),
                MouseButton.Middle => CurrentMouseState.MiddleButton == ButtonState.Pressed && PreviousMouseState.MiddleButton == ButtonState.Released,
                _ => false
            };
        }

        public bool IsMouseButtonDown(MouseButton button)
        {
            return button switch
            {
                MouseButton.Left => IsLeftMouseDown(),
                MouseButton.Right => IsRightMouseDown(),
                MouseButton.Middle => CurrentMouseState.MiddleButton == ButtonState.Pressed,
                _ => false
            };
        }

        public int GetScrollWheelDelta()
        {
            return CurrentMouseState.ScrollWheelValue - PreviousMouseState.ScrollWheelValue;
        }

        public bool IsMouseInBounds(Rectangle bounds)
        {
            return bounds.Contains((int)MousePosition.X, (int)MousePosition.Y);
        }

        // rendering helpers for components
        public void DrawRectangle(Rectangle rect, Color color)
        {
            SpriteBatch.Draw(PixelTexture, rect, color);
        }

        public void DrawRectangleOutline(Rectangle rect, Color color, int thickness = 1)
        {
            // top
            SpriteBatch.Draw(PixelTexture, new Rectangle(rect.X, rect.Y, rect.Width, thickness), color);
            // bottom
            SpriteBatch.Draw(PixelTexture, new Rectangle(rect.X, rect.Y + rect.Height - thickness, rect.Width, thickness), color);
            // left
            SpriteBatch.Draw(PixelTexture, new Rectangle(rect.X, rect.Y, thickness, rect.Height), color);
            // right
            SpriteBatch.Draw(PixelTexture, new Rectangle(rect.X + rect.Width - thickness, rect.Y, thickness, rect.Height), color);
        }

        // ui state management
        public void Show()
        {
            IsVisible = true;
            Engine.Instance.IsMouseVisible = true;
        }

        public void Hide()
        {
            IsVisible = false;
        }

        public void Toggle()
        {
            if (IsVisible)
                Hide();
            else
                Show();
        }
    }

    public enum MouseButton
    {
        Left,
        Right,
        Middle
    }
}
