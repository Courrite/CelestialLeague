using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CelestialLeague.Client.UI.Core
{
    public class UILayer: UIComponent
    {
        // identity
        public string Name { get; set; }
        public int Depth { get; set; }

        // layer state
        public bool AcceptsInput { get; set; } = true;
        public bool IsModal { get; set; } = false; // modal layers block input to layers below

        // components
        private List<UIComponent> components;

        // layer-wide properties
        public Vector2 Offset { get; set; } = Vector2.Zero; // for screen shake, transitions, etc.

        // events
        public event Action<UILayer> LayerShown;
        public event Action<UILayer> LayerHidden;
        public event Action<UILayer, UIComponent> ComponentAdded;
        public event Action<UILayer, UIComponent> ComponentRemoved;

        public UILayer(string name, int depth = 0)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Depth = depth;
            components = [];
        }

        // component management
        public void AddComponent(UIComponent component)
        {
            if (component == null) return;

            UIManager.Instance?.RemoveComponentFromAllLayers(component);

            components.Add(component);
            ComponentAdded?.Invoke(this, component);
        }

        public void RemoveComponent(UIComponent component)
        {
            if (component == null) return;

            if (components.Remove(component))
            {
                ComponentRemoved?.Invoke(this, component);
            }
        }

        public void ClearComponents()
        {
            foreach (var component in components)
            {
                component.Cleanup();
                ComponentRemoved?.Invoke(this, component);
            }
            components.Clear();
        }

        // queries
        public T FindComponent<T>(string id) where T : UIComponent
        {
            return FindComponent(id) as T;
        }

        public UIComponent FindComponent(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;

            foreach (var component in components)
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

        public IEnumerable<UIComponent> GetAllComponents()
        {
            foreach (var component in components)
            {
                yield return component;
                foreach (var child in GetAllChildrenRecursive(component))
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

        // visibility
        public void Show()
        {
            if (IsVisible) return;

            IsVisible = true;
            LayerShown?.Invoke(this);
        }

        public void Hide()
        {
            if (!IsVisible) return;

            IsVisible = false;
            LayerHidden?.Invoke(this);
        }

        public void Toggle()
        {
            if (IsVisible)
                Hide();
            else
                Show();
        }

        // ordering
        public void BringToFront()
        {
            UIManager.Instance?.BringLayerToFront(this);
        }

        public void SendToBack()
        {
            UIManager.Instance?.SendLayerToBack(this);
        }

        // modal layer support
        public void SetModal(bool modal)
        {
            IsModal = modal;
            UIManager.Instance?.OnLayerModalChanged(this);
        }

        // layer stats
        public int ComponentCount => components.Count;
        public int TotalComponentCount => GetAllComponents().Count();

        public bool HasFocusedComponent()
        {
            return GetAllComponents().Any(c => c.IsFocused);
        }

        public bool HasHoveredComponent()
        {
            return GetAllComponents().Any(c => c.IsHovered);
        }

        public override void Cleanup()
        {
            base.Cleanup();
            
            LayerShown = null;
            LayerHidden = null;
            ComponentAdded = null;
            ComponentRemoved = null;

            ClearComponents();
        }

        // debug helper
        public override string ToString()
        {
            return $"UILayer '{Name}' (Depth: {Depth}, Components: {ComponentCount}, Visible: {IsVisible})";
        }
    }
}
