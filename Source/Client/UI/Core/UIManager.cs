using Monocle;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using Celeste.Mod;
using Celeste;

namespace CelestialLeague.Client.UI.Core
{
    public class UIManager : Entity
    {
        public static UIManager Instance { get; private set; }

        // core ui state
        public bool IsVisible { get; set; } = true;
        public bool AcceptInput { get; set; } = true;

        // component management
        private List<UIComponent> rootComponents;

        // input state tracking
        private UIComponent focusedComponent;
        private UIComponent hoveredComponent;
        private Vector2 lastMousePosition;
        private bool lastMousePressed;
        private bool lastRightMousePressed;

        // key state tracking
        private KeyboardState previousKeyboardState;
        private KeyboardState currentKeyboardState;
        private HashSet<Keys> pressedThisFrame;
        private HashSet<Keys> releasedThisFrame;

        // rendering
        private SpriteBatch spriteBatch;
        private bool spriteBatchInitialized = false;

        // focus management
        private List<UIComponent> focusableComponents;

        // events
        public event Action<UIComponent> ComponentFocused;
        public event Action<UIComponent> ComponentClicked;

        // input viewer api
        public event Action<Keys> KeyPressed;
        public event Action<Keys> KeyReleased;
        public event Action<Keys> KeyHeld;

        public UIManager() : base()
        {
            Instance = this;

            rootComponents = new List<UIComponent>();
            focusableComponents = new List<UIComponent>();
            pressedThisFrame = new HashSet<Keys>();
            releasedThisFrame = new HashSet<Keys>();

            currentKeyboardState = Keyboard.GetState();
            previousKeyboardState = currentKeyboardState;

            Tag = new BitTag("UI");
            Depth = -1000;

            Logger.Log(LogLevel.Info, "CelestialLeague", "UIManager initialized (SpriteBatch will be created later)");
        }

        private void EnsureSpriteBatchInitialized()
        {
            if (!spriteBatchInitialized && Engine.Graphics?.GraphicsDevice != null)
            {
                spriteBatch = new SpriteBatch(Engine.Graphics.GraphicsDevice);
                spriteBatchInitialized = true;
                Logger.Log(LogLevel.Info, "CelestialLeague", "SpriteBatch initialized");
            }
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            EnsureSpriteBatchInitialized();
            Logger.Log(LogLevel.Info, "CelestialLeague", "UIManager added to scene");
        }

        public override void Update()
        {
            if (!IsVisible) return;

            EnsureSpriteBatchInitialized();
            base.Update();

            UpdateKeyboardState();
            UpdateComponents();
            if (AcceptInput)
            {
                HandleInput();
            }
            RefreshFocusableComponents();
        }

        private void UpdateKeyboardState()
        {
            previousKeyboardState = currentKeyboardState;
            currentKeyboardState = Keyboard.GetState();

            pressedThisFrame.Clear();
            releasedThisFrame.Clear();

            var allKeys = Enum.GetValues<Keys>();

            foreach (Keys key in allKeys)
            {
                bool wasPressed = previousKeyboardState.IsKeyDown(key);
                bool isPressed = currentKeyboardState.IsKeyDown(key);

                if (isPressed && !wasPressed)
                {
                    pressedThisFrame.Add(key);
                    KeyPressed?.Invoke(key);
                }
                else if (!isPressed && wasPressed)
                {
                    releasedThisFrame.Add(key);
                    KeyReleased?.Invoke(key);
                }
                else if (isPressed && wasPressed)
                {
                    KeyHeld?.Invoke(key);
                }
            }
        }

        private void UpdateComponents()
        {
            for (int i = rootComponents.Count - 1; i >= 0; i--)
            {
                if (i < rootComponents.Count)
                {
                    var component = rootComponents[i];
                    if (component.IsVisible)
                    {
                        component.Update();
                    }
                }
            }
        }

        private void HandleInput()
        {
            var mouseState = MInput.Mouse.CurrentState;
            var mousePosition = new Vector2(mouseState.X, mouseState.Y);
            bool leftPressed = mouseState.LeftButton == ButtonState.Pressed;
            bool rightPressed = mouseState.RightButton == ButtonState.Pressed;

            if (mousePosition != lastMousePosition)
            {
                HandleMouseMove(mousePosition);
                lastMousePosition = mousePosition;
            }

            if (leftPressed && !lastMousePressed)
            {
                HandleMouseDown(mousePosition, true);
            }
            else if (!leftPressed && lastMousePressed)
            {
                HandleMouseUp(mousePosition, true);
            }

            if (rightPressed && !lastRightMousePressed)
            {
                HandleMouseDown(mousePosition, false);
            }
            else if (!rightPressed && lastRightMousePressed)
            {
                HandleMouseUp(mousePosition, false);
            }

            if (focusedComponent != null)
            {
                HandleKeyboardInput();
            }

            lastMousePressed = leftPressed;
            lastRightMousePressed = rightPressed;
        }

        private void HandleMouseMove(Vector2 mousePosition)
        {
            UIComponent newHovered = FindComponentAt(mousePosition);

            if (newHovered != hoveredComponent)
            {
                if (hoveredComponent != null)
                {
                    hoveredComponent.OnMouseLeave();
                }

                hoveredComponent = newHovered;

                if (hoveredComponent != null)
                {
                    hoveredComponent.OnMouseEnter();
                }
            }

            if (hoveredComponent != null)
            {
                hoveredComponent.OnMouseMove(mousePosition);
            }
        }

        private void HandleMouseDown(Vector2 mousePosition, bool isLeftButton)
        {
            if (!isLeftButton) return;

            UIComponent clickedComponent = FindComponentAt(mousePosition);

            SetFocusedComponent(clickedComponent);

            if (clickedComponent != null && clickedComponent.CanReceiveInput && clickedComponent.IsEnabled)
            {
                clickedComponent.OnMouseDown(mousePosition);
            }
        }

        private void HandleMouseUp(Vector2 mousePosition, bool isLeftButton)
        {
            if (!isLeftButton) return;

            UIComponent releasedComponent = FindComponentAt(mousePosition);

            var allComponents = GetAllComponents().ToList();
            foreach (var component in allComponents)
            {
                if (component.IsPressed)
                {
                    component.OnMouseUp(mousePosition);

                    if (component == releasedComponent && component.CanReceiveInput && component.IsEnabled)
                    {
                        component.OnClick(mousePosition);
                        ComponentClicked?.Invoke(component);
                    }
                }
            }
        }

        private void HandleKeyboardInput()
        {
            foreach (var key in pressedThisFrame)
            {
                focusedComponent.OnKeyPressed(key);

                if (key == Keys.Tab)
                {
                    bool shiftHeld = currentKeyboardState.IsKeyDown(Keys.LeftShift) || currentKeyboardState.IsKeyDown(Keys.RightShift);
                    if (shiftHeld)
                        MoveFocusToPrevious();
                    else
                        MoveFocusToNext();
                }
            }

            foreach (var key in releasedThisFrame)
            {
                focusedComponent.OnKeyReleased(key);
            }
        }

        private UIComponent FindComponentAt(Vector2 position)
        {
            for (int i = rootComponents.Count - 1; i >= 0; i--)
            {
                var component = rootComponents[i];
                if (!component.IsVisible) continue;

                var found = component.GetComponentAt(new Point((int)position.X, (int)position.Y));
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        public override void Render()
        {
            if (!IsVisible || !spriteBatchInitialized) return;

            spriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                SamplerState.PointClamp,
                DepthStencilState.None,
                RasterizerState.CullNone,
                null,
                Matrix.Identity
            );

            foreach (var component in rootComponents)
            {
                if (component.IsVisible)
                {
                    component.Render(spriteBatch);
                }
            }

            spriteBatch.End();
        }

        public void RenderUI()
        {
            if (!IsVisible || !spriteBatchInitialized) return;

            spriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                SamplerState.PointClamp,
                DepthStencilState.None,
                RasterizerState.CullNone
            );

            foreach (var component in rootComponents)
            {
                if (component.IsVisible)
                {
                    component.Render(spriteBatch);
                }
            }

            spriteBatch.End();
        }


        // input viewer api
        public bool IsKeyPressed(Keys key) => pressedThisFrame.Contains(key);
        public bool IsKeyReleased(Keys key) => releasedThisFrame.Contains(key);
        public bool IsKeyHeld(Keys key) => currentKeyboardState.IsKeyDown(key);
        public bool WasKeyHeld(Keys key) => previousKeyboardState.IsKeyDown(key);

        public IEnumerable<Keys> GetPressedKeys() => pressedThisFrame;
        public IEnumerable<Keys> GetReleasedKeys() => releasedThisFrame;
        public IEnumerable<Keys> GetHeldKeys() => Enum.GetValues<Keys>().Where(k => currentKeyboardState.IsKeyDown(k));

        // component management
        public void AddComponent(UIComponent component)
        {
            if (component == null) return;
            component.RemoveFromParent();
            rootComponents.Add(component);
            RefreshFocusableComponents();
            Logger.Log(LogLevel.Verbose, "CelestialLeague", $"Added UI component: {component}");
        }

        public void RemoveComponent(UIComponent component)
        {
            if (component == null) return;
            if (rootComponents.Remove(component))
            {
                if (focusedComponent == component)
                    focusedComponent = null;
                if (hoveredComponent == component)
                    hoveredComponent = null;
                component.Cleanup();
                RefreshFocusableComponents();
                Logger.Log(LogLevel.Verbose, "CelestialLeague", $"Removed UI component: {component}");
            }
        }

        public void ClearComponents()
        {
            focusedComponent = null;
            hoveredComponent = null;
            foreach (var component in rootComponents)
            {
                component.Cleanup();
            }
            rootComponents.Clear();
            focusableComponents.Clear();
            Logger.Log(LogLevel.Info, "CelestialLeague", "Cleared all UI components");
        }

        // focus management
        public void SetFocusedComponent(UIComponent component)
        {
            if (focusedComponent == component) return;
            if (focusedComponent != null)
            {
                focusedComponent.OnLostFocus();
            }
            focusedComponent = component;
            if (focusedComponent != null && focusedComponent.IsFocusable && focusedComponent.IsEnabled)
            {
                focusedComponent.OnGainedFocus();
                ComponentFocused?.Invoke(focusedComponent);
            }
            else
            {
                focusedComponent = null;
            }
        }

        public void MoveFocusToNext()
        {
            if (focusableComponents.Count == 0) return;
            int currentIndex = focusedComponent != null ? focusableComponents.IndexOf(focusedComponent) : -1;
            int nextIndex = (currentIndex + 1) % focusableComponents.Count;
            SetFocusedComponent(focusableComponents[nextIndex]);
        }

        public void MoveFocusToPrevious()
        {
            if (focusableComponents.Count == 0) return;
            int currentIndex = focusedComponent != null ? focusableComponents.IndexOf(focusedComponent) : 0;
            int prevIndex = currentIndex <= 0 ? focusableComponents.Count - 1 : currentIndex - 1;
            SetFocusedComponent(focusableComponents[prevIndex]);
        }

        private void RefreshFocusableComponents()
        {
            focusableComponents.Clear();
            foreach (var component in GetAllComponents())
            {
                if (component.IsFocusable && component.IsEnabled && component.IsVisible)
                {
                    focusableComponents.Add(component);
                }
            }
        }

        public void BringLayerToFront(UILayer layer)
        {
            UIComponent layerComponent = layer;
            if (layerComponent != null && rootComponents.Remove(layerComponent))
            {
                rootComponents.Add(layerComponent);
            }
        }

        public void SendLayerToBack(UILayer layer)
        {
            UIComponent layerComponent = layer;
            if (layerComponent != null && rootComponents.Remove(layerComponent))
            {
                rootComponents.Insert(0, layerComponent);
            }
        }

        public void OnLayerModalChanged(UILayer layer)
        {
            if (layer.IsModal)
            {
                foreach (var component in rootComponents)
                {
                    if (component != layer)
                    {
                        component.CanReceiveInput = false;
                    }
                }
            }
            else
            {
                foreach (var component in rootComponents)
                {
                    component.CanReceiveInput = true;
                }
            }
        }

        public void RemoveComponentFromAllLayers(UIComponent component)
        {
            rootComponents.Remove(component);

            foreach (var rootComponent in rootComponents)
            {
                if (rootComponent is UILayer layer)
                {
                    layer.RemoveComponent(component);
                }
            }
        }

        // util methods
        public T FindComponent<T>(string id) where T : UIComponent
        {
            return FindComponent(id) as T;
        }

        public UIComponent FindComponent(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            foreach (var component in rootComponents)
            {
                if (component.Id == id) return component;
                var found = component.FindChild(id);
                if (found != null) return found;
            }
            return null;
        }

        public IEnumerable<T> GetComponentsOfType<T>() where T : UIComponent
        {
            return GetAllComponents().OfType<T>();
        }

        private IEnumerable<UIComponent> GetAllComponents()
        {
            foreach (var root in rootComponents)
            {
                yield return root;
                foreach (var child in GetAllChildrenRecursive(root))
                {
                    yield return child;
                }
            }
        }

        private IEnumerable<UIComponent> GetAllChildrenRecursive(UIComponent parent)
        {
            foreach (var child in parent.Children)
            {
                yield return child;
                foreach (var grandchild in GetAllChildrenRecursive(child))
                {
                    yield return grandchild;
                }
            }
        }

        // stats
        public int ComponentCount => rootComponents.Count;
        public int TotalComponentCount => GetAllComponents().Count();
        public UIComponent GetFocusedComponent() => focusedComponent;
        public UIComponent GetHoveredComponent() => hoveredComponent;

        internal void OnComponentFocusRequested(UIComponent component)
        {
            SetFocusedComponent(component);
        }

        public override void Removed(Scene scene)
        {
            ClearComponents();

            KeyPressed = null;
            KeyReleased = null;
            KeyHeld = null;

            ComponentFocused = null;
            ComponentClicked = null;

            spriteBatch?.Dispose();
            spriteBatchInitialized = false;

            if (Instance == this)
                Instance = null;
            Logger.Log(LogLevel.Info, "CelestialLeague", "UIManager removed from scene");
            base.Removed(scene);
        }

        // debug helper
        public override string ToString()
        {
            return $"UIManager (Components: {ComponentCount}, Focused: {focusedComponent?.Id ?? "none"})";
        }
    }
}
