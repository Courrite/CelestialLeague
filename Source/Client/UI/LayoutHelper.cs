using Celeste.Mod;
using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace CelestialLeague.Client.UI
{
    public static class LayoutHelper
    {
        public static Vector2 CalculatePosition(IUIComponent component)
        {
            var layout = component.Layout;
            Vector2 parentSize = GetParentSize(component);
            Vector2 resolvedPosition = layout.Position.Resolve(parentSize);
            var contentSize = CalculateSize(component);
            Vector2 pivotOffset = GetAnchorOffset(contentSize, layout.Anchor);
            Vector2 finalPosition = resolvedPosition - pivotOffset;
            return finalPosition;
        }

        public static Vector2 CalculateSize(IUIComponent component)
        {
            var layout = component.Layout;
            Vector2 parentSize = GetParentSize(component);
            Vector2 size = layout.Size.Resolve(parentSize);
            return size;
        }

        private static Vector2 GetAnchorOffset(Vector2 size, Vector2 anchor)
        {
            return new Vector2(
                MathHelper.Lerp(0, size.X, anchor.X),
                MathHelper.Lerp(0, size.Y, anchor.Y)
            );
        }

        private static Vector2 GetParentSize(IUIComponent component)
        {
            if (component.Parent == null)
            {
                var viewport = Engine.Graphics.GraphicsDevice.Viewport;
                return new Vector2(viewport.Width, viewport.Height);
            }

            return new Vector2(component.Parent.Bounds.Width, component.Parent.Bounds.Height);
        }

        public static void DebugLayout(IUIComponent component, string name = "Component")
        {
            var parent = component.Parent;
            var layout = component.Layout;
            var position = CalculatePosition(component);
            var size = CalculateSize(component);
            var bounds = new Rectangle((int)position.X, (int)position.Y, (int)size.X, (int)size.Y);

            Console.WriteLine($"{name}:");
            Console.WriteLine($"  Position: {position}");
            Console.WriteLine($"  Size: {size}");
            Console.WriteLine($"  Bounds: {bounds}");
            Console.WriteLine($"  Anchor: {layout.Anchor}");
            Console.WriteLine($"  Parent: {parent}");

            if (component.Parent != null)
            {
                var parentBounds = component.Parent.Bounds;
                System.Console.WriteLine($"  Parent Bounds: {parentBounds}");
            }

            var pivotOffset = GetAnchorOffset(size, layout.Anchor);
            Console.WriteLine($"  Pivot Offset: {pivotOffset}");
            Console.WriteLine();
        }
    }
}
