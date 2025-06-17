using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.CelestialLeague.Client.UI.Core
{
    public class UIManager : Entity
    {
        private static UIManager instance;
        public static UIManager Instance => instance;

        private List<UILayer> layers;
        private List<UIComponent> inputHandlers;
        private Stack<UIComponent> stateStack;
        private UIComponent currentScreen;
        private SpriteBatch spriteBatch;
        private bool isInitialized;

        // input state tracking
        private Vector2 lastMousePosition;
        private bool lastMousePressed;
        private UIComponent focusedComponent;
        private UIComponent hoveredComponent;

        public UIManager()
        {
            instance = this;
            layers = new List<UILayer>();
            inputHandlers = new List<UIComponent>();
            stateStack = new Stack<UIComponent>();
            Tag = Tags.Global | Tags.Persistent;
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Initialize();
        }

        public void Initialize()
        {
            if (isInitialized) return;

            AddLayer(new UILayer("Background", 0));
            AddLayer(new UILayer("Main", 100));
            AddLayer(new UILayer("Overlay", 200));
            AddLayer(new UILayer("Modal", 300));
            AddLayer(new UILayer("Tooltip", 400));

            isInitialized = true;
            Logger.Log(LogLevel.Info, "CelestialLeague", "UIManager initialized");
        }

        public void LoadContent()
        {
            spriteBatch = new SpriteBatch(Engine.Graphics.GraphicsDevice);
            
            foreach (var layer in layers)
            {
                layer.LoadContent();
            }
        }

        public override void Update()
        {
            base.Update();
            
            if (!isInitialized) return;

            HandleInput();
            
            // update layers in reverse order (top to bottom for input)
            for (int i = layers.Count - 1; i >= 0; i--)
            {
                if (layers[i].IsVisible)
                {
                    layers[i].Update();
                }
            }

            currentScreen?.Update();
        }

        public override void Render()
        {
            base.Render();
            
            if (!isInitialized || spriteBatch == null) return;

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, 
                            SamplerState.PointClamp, DepthStencilState.None, 
                            RasterizerState.CullNone);

            // draw layers in order (bottom to top)
            foreach (var layer in layers.OrderBy(l => l.ZOrder))
            {
                if (layer.IsVisible)
                {
                    layer.Draw(spriteBatch);
                }
            }

            spriteBatch.End();
        }

        public void AddLayer(UILayer layer)
        {
            if (layer == null) return;
            
            layers.Add(layer);
            layers.Sort((a, b) => a.ZOrder.CompareTo(b.ZOrder));
            
            Logger.Log(LogLevel.Verbose, "CelestialLeague", $"Added UI layer: {layer.Name}");
        }

        public void RemoveLayer(UILayer layer)
        {
            if (layer == null) return;
            
            layer.Cleanup();
            layers.Remove(layer);
            
            Logger.Log(LogLevel.Verbose, "CelestialLeague", $"Removed UI layer: {layer.Name}");
        }

        public UILayer GetLayer(string layerName)
        {
            return layers.FirstOrDefault(l => l.Name == layerName);
        }

        public void SetLayerVisibility(string layerName, bool visible)
        {
            var layer = GetLayer(layerName);
            if (layer != null)
            {
                layer.IsVisible = visible;
            }
        }

        public void ClearAllLayers()
        {
            foreach (var layer in layers)
            {
                layer.Cleanup();
            }
            layers.Clear();
        }

        public void ShowScreen(UIComponent screen)
        {
            if (screen == null) return;

            if (currentScreen != null)
            {
                currentScreen.IsVisible = false;
                currentScreen.OnHidden();
            }

            currentScreen = screen;
            screen.IsVisible = true;
            screen.OnShown();

            var mainLayer = GetLayer("Main");
            if (mainLayer != null && !mainLayer.Contains(screen))
            {
                mainLayer.AddComponent(screen);
            }

            Logger.Log(LogLevel.Info, "CelestialLeague", $"Showing screen: {screen.GetType().Name}");
        }

        public void HideScreen(UIComponent screen)
        {
            if (screen == null) return;

            screen.IsVisible = false;
            screen.OnHidden();

            if (currentScreen == screen)
            {
                currentScreen = null;
            }

            Logger.Log(LogLevel.Info, "CelestialLeague", $"Hiding screen: {screen.GetType().Name}");
        }

        public UIComponent GetCurrentScreen()
        {
            return currentScreen;
        }

        public void TransitionToScreen(UIComponent newScreen)
        {
            ShowScreen(newScreen);
        }

        private void HandleInput()
        {
            var mouseState = MInput.Mouse.CurrentState;
            var mousePosition = new Vector2(mouseState.X, mouseState.Y);
            bool mousePressed = mouseState.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed;

            HandleMouseHover(mousePosition);

            if (mousePressed && !lastMousePressed)
            {
                HandleMouseClick(mousePosition);
            }
			
            if (focusedComponent != null)
			{
				HandleKeyboardInput();
			}

            lastMousePosition = mousePosition;
            lastMousePressed = mousePressed;
        }

        private void HandleMouseHover(Vector2 mousePosition)
        {
            UIComponent newHovered = null;

            // check components in reverse order (top to bottom)
            for (int i = layers.Count - 1; i >= 0; i--)
            {
                if (!layers[i].IsVisible) continue;

                newHovered = layers[i].GetComponentAt(mousePosition);
                if (newHovered != null) break;
            }

            if (hoveredComponent != newHovered)
            {
                hoveredComponent?.OnMouseLeave();
                hoveredComponent = newHovered;
                hoveredComponent?.OnMouseEnter();
            }
        }

        private void HandleMouseClick(Vector2 mousePosition)
        {
            UIComponent clickedComponent = null;

            // topmost clickable component
            for (int i = layers.Count - 1; i >= 0; i--)
            {
                if (!layers[i].IsVisible) continue;

                clickedComponent = layers[i].GetComponentAt(mousePosition);
                if (clickedComponent != null && clickedComponent.CanReceiveInput)
                {
                    break;
                }
            }

            if (clickedComponent != null)
            {
                SetFocus(clickedComponent);
                clickedComponent.OnMouseClick(mousePosition);
            }
            else
            {
                // click on empty space - clear focus
                SetFocus(null);
            }
        }

        private void HandleKeyboardInput()
        {
            if (MInput.Keyboard.Pressed(Microsoft.Xna.Framework.Input.Keys.Tab))
            {
                HandleTabNavigation();
            }
            else if (MInput.Keyboard.Pressed(Microsoft.Xna.Framework.Input.Keys.Enter))
            {
                focusedComponent?.OnKeyPressed(Microsoft.Xna.Framework.Input.Keys.Enter);
            }
            else if (MInput.Keyboard.Pressed(Microsoft.Xna.Framework.Input.Keys.Escape))
            {
                focusedComponent?.OnKeyPressed(Microsoft.Xna.Framework.Input.Keys.Escape);
            }

            // let focused component handle other keys
            focusedComponent?.HandleKeyboardInput();
        }

        private void HandleTabNavigation()
        {
            var tabbableComponents = GetTabbableComponents();
            if (tabbableComponents.Count == 0) return;

            int currentIndex = focusedComponent != null ? 
                tabbableComponents.IndexOf(focusedComponent) : -1;
            
            int nextIndex = (currentIndex + 1) % tabbableComponents.Count;
            SetFocus(tabbableComponents[nextIndex]);
        }

        private List<UIComponent> GetTabbableComponents()
        {
            var components = new List<UIComponent>();
            
            foreach (var layer in layers.OrderBy(l => l.ZOrder))
            {
                if (layer.IsVisible)
                {
                    components.AddRange(layer.GetTabbableComponents());
                }
            }
            
            return components;
        }

        public void RegisterInputHandler(UIComponent component)
        {
            if (component != null && !inputHandlers.Contains(component))
            {
                inputHandlers.Add(component);
            }
        }

        public void UnregisterInputHandler(UIComponent component)
        {
            inputHandlers.Remove(component);
            
            if (focusedComponent == component)
            {
                focusedComponent = null;
            }
            
            if (hoveredComponent == component)
            {
                hoveredComponent = null;
            }
        }

        public void SetFocus(UIComponent component)
        {
            if (focusedComponent == component) return;

            focusedComponent?.OnLostFocus();
            focusedComponent = component;
            focusedComponent?.OnGainedFocus();
        }

        public void PushState(UIComponent state)
        {
            if (currentScreen != null)
            {
                stateStack.Push(currentScreen);
                currentScreen.IsVisible = false;
            }
            
            ShowScreen(state);
        }

        public void PopState()
        {
            if (stateStack.Count == 0) return;

            if (currentScreen != null)
            {
                HideScreen(currentScreen);
            }

            var previousState = stateStack.Pop();
            ShowScreen(previousState);
        }

        public UIComponent GetCurrentState()
        {
            return stateStack.Count > 0 ? stateStack.Peek() : null;
        }
		
        public UIComponent FindComponent(string componentId)
		{
			foreach (var layer in layers)
			{
				var component = layer.FindComponent(componentId);
				if (component != null) return component;
			}
			return null;
		}

        public void BringToFront(UIComponent component)
        {
            foreach (var layer in layers)
            {
                if (layer.Contains(component))
                {
                    layer.BringToFront(component);
                    break;
                }
            }
        }

        public void SendToBack(UIComponent component)
        {
            foreach (var layer in layers)
            {
                if (layer.Contains(component))
                {
                    layer.SendToBack(component);
                    break;
                }
            }
        }

        public bool IsComponentVisible(UIComponent component)
        {
            if (component == null || !component.IsVisible) return false;

            foreach (var layer in layers)
            {
                if (layer.Contains(component))
                {
                    return layer.IsVisible;
                }
            }
            
            return false;
        }
		
        public override void Removed(Scene scene)
		{
			Unload();
			base.Removed(scene);
		}

        public void Unload()
        {
            ClearAllLayers();
            inputHandlers.Clear();
            stateStack.Clear();
            currentScreen = null;
            focusedComponent = null;
            hoveredComponent = null;
            
            spriteBatch?.Dispose();
            spriteBatch = null;
            
            UIComponent.DisposeStaticResources();
            
            isInitialized = false;
            instance = null;
            
            Logger.Log(LogLevel.Info, "CelestialLeague", "UIManager unloaded");
        }
    }
}