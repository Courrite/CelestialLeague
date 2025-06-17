using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.CelestialLeague.Client.UI.Core
{
	public class UILayer
	{
		public string Name { get; private set; }
		public int ZOrder { get; set; }
		public bool IsVisible { get; set; } = true;
		public bool IsInteractive { get; set; } = true;
		public Color TintColor { get; set; } = Color.White;
		public float Alpha { get; set; } = 1.0f;

		private List<UIComponent> components;
		private bool needsSorting;

		public UILayer(string name, int zOrder = 0)
		{
			Name = name;
			ZOrder = zOrder;
			components = new List<UIComponent>();
		}

		public void AddComponent(UIComponent component)
		{
			if (component == null || components.Contains(component)) return;

			components.Add(component);
			needsSorting = true;

			Logger.Log(LogLevel.Verbose, "CelestialLeague", $"Added component {component.Id} to layer {Name}");
		}

		public void RemoveComponent(UIComponent component)
		{
			if (component == null) return;

			components.Remove(component);
			component.Cleanup();

			Logger.Log(LogLevel.Verbose, "CelestialLeague", $"Removed component {component.Id} from layer {Name}");
		}

		public void RemoveAllComponents()
		{
			var componentsCopy = new List<UIComponent>(components);
			foreach (var component in componentsCopy)
			{
				RemoveComponent(component);
			}
		}

		public bool Contains(UIComponent component)
		{
			return components.Contains(component);
		}

		public UIComponent FindComponent(string componentId)
		{
			return components.FirstOrDefault(c => c.Id == componentId);
		}

		public List<UIComponent> GetComponents()
		{
			return new List<UIComponent>(components);
		}

		public List<UIComponent> GetTabbableComponents()
		{
			return components.Where(c => c.IsVisible && c.IsEnabled && c.IsFocusable)
						   .OrderBy(c => c.Position.Y)
						   .ThenBy(c => c.Position.X)
						   .ToList();
		}

		public UIComponent GetComponentAt(Vector2 point)
		{
			if (!IsVisible || !IsInteractive) return null;

			// check components in reverse order top to bottom
			for (int i = components.Count - 1; i >= 0; i--)
			{
				var component = components[i].GetComponentAt(point);
				if (component != null) return component;
			}

			return null;
		}

		public void Update()
		{
			if (!IsVisible) return;

			SortComponentsIfNeeded();

			foreach (var component in components)
			{
				component.Update();
			}
		}

		public void Draw(SpriteBatch spriteBatch)
		{
			if (!IsVisible) return;

			SortComponentsIfNeeded();

			var originalColor = TintColor * Alpha;

			foreach (var component in components)
			{
				if (component.IsVisible)
				{
					component.Draw(spriteBatch);
				}
			}
		}

		public void LoadContent()
		{
			// override in derived classes if needed
			foreach (var component in components)
			{
				// components can load their own content
			}
		}

		public void BringToFront(UIComponent component)
		{
			if (!components.Contains(component)) return;

			components.Remove(component);
			components.Add(component);

			Logger.Log(LogLevel.Verbose, "CelestialLeague", $"Brought component {component.Id} to front in layer {Name}");
		}

		public void SendToBack(UIComponent component)
		{
			if (!components.Contains(component)) return;

			components.Remove(component);
			components.Insert(0, component);

			Logger.Log(LogLevel.Verbose, "CelestialLeague", $"Sent component {component.Id} to back in layer {Name}");
		}

		public void SetComponentZOrder(UIComponent component, int index)
		{
			if (!components.Contains(component)) return;

			components.Remove(component);
			index = Math.Max(0, Math.Min(index, components.Count));
			components.Insert(index, component);

			Logger.Log(LogLevel.Verbose, "CelestialLeague", $"Set component {component.Id} z-order to {index} in layer {Name}");
		}

		private void SortComponentsIfNeeded()
		{
			if (!needsSorting) return;

			// sort by Y position first, then X position for natural tab order
			components.Sort((a, b) =>
			{
				int yCompare = a.Position.Y.CompareTo(b.Position.Y);
				return yCompare != 0 ? yCompare : a.Position.X.CompareTo(b.Position.X);
			});

			needsSorting = false;
		}

		public void Cleanup()
		{
			RemoveAllComponents();
			Logger.Log(LogLevel.Verbose, "CelestialLeague",
				$"Cleaned up layer {Name}");
		}

		public int GetComponentCount()
		{
			return components.Count;
		}

		public bool IsEmpty()
		{
			return components.Count == 0;
		}

		public void SetAlpha(float alpha)
		{
			Alpha = MathHelper.Clamp(alpha, 0f, 1f);
		}

		public void Show()
		{
			IsVisible = true;
		}

		public void Hide()
		{
			IsVisible = false;
		}

		public void EnableInteraction()
		{
			IsInteractive = true;
		}

		public void DisableInteraction()
		{
			IsInteractive = false;
		}

		public void LogComponents()
		{
			Logger.Log(LogLevel.Info, "CelestialLeague", $"Layer {Name} contains {components.Count} components:");

			for (int i = 0; i < components.Count; i++)
			{
				var component = components[i];
				Logger.Log(LogLevel.Info, "CelestialLeague",$"  [{i}] {component.Id} - Visible: {component.IsVisible}, Enabled: {component.IsEnabled}");
			}
		}

		public override string ToString()
		{
			return $"UILayer[{Name}] - ZOrder: {ZOrder}, Components: {components.Count}, Visible: {IsVisible}";
		}
	}
}