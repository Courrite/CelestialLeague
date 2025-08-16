using Celeste.Mod;
using Microsoft.Xna.Framework;

namespace CelestialLeague.Client.UI
{
    public static class LayoutHelper
    {
        public static Vector2 CalculatePosition(IUIComponent component)
        {
            var layout = component.Layout;

            Vector2 basePosition = CalculateBasePosition(component);

            var contentSize = CalculateSize(component);

            Vector2 pivotOffset = GetAnchorOffset(contentSize, layout.Anchor);
            basePosition -= pivotOffset;

            basePosition += new Vector2(layout.Margin.Left, layout.Margin.Top);

            if (layout.Offset.HasValue)
                basePosition += layout.Offset.Value;

            return basePosition;
        }

        public static Vector2 CalculateSize(IUIComponent component)
        {
            var layout = component.Layout;
            Vector2 size;

            if (layout.RelativeSize.HasValue && component.Parent != null)
            {
                var parentBounds = component.Parent.ContentBounds;
                size = new Vector2(
                    parentBounds.Width * layout.RelativeSize.Value.X,
                    parentBounds.Height * layout.RelativeSize.Value.Y
                );
            }
            else if (layout.FillParent && component.Parent != null)
            {
                var parentBounds = component.Parent.ContentBounds;
                size = new Vector2(
                    parentBounds.Width - layout.Margin.Left - layout.Margin.Right,
                    parentBounds.Height - layout.Margin.Top - layout.Margin.Bottom
                );
            }
            else
            {
                size = Vector2.Zero;
            }

            if (layout.AbsoluteSize.HasValue)
            {
                size += layout.AbsoluteSize.Value;
            }

            if (layout.MinSize.HasValue)
            {
                size.X = MathHelper.Max(size.X, layout.MinSize.Value.X);
                size.Y = MathHelper.Max(size.Y, layout.MinSize.Value.Y);
            }
            if (layout.MaxSize.HasValue)
            {
                size.X = MathHelper.Min(size.X, layout.MaxSize.Value.X);
                size.Y = MathHelper.Min(size.Y, layout.MaxSize.Value.Y);
            }

            return size;
        }

        private static Vector2 CalculateBasePosition(IUIComponent component)
        {
            var layout = component.Layout;

            if (layout.AbsolutePosition.HasValue)
            {
                return layout.AbsolutePosition.Value;
            }

            if (component.Parent == null)
                return Vector2.Zero;

            var parentBounds = component.Parent.ContentBounds;
            Vector2 position = new Vector2(parentBounds.X, parentBounds.Y);

            if (layout.RelativePosition.HasValue)
            {
                position += new Vector2(
                    parentBounds.Width * layout.RelativePosition.Value.X,
                    parentBounds.Height * layout.RelativePosition.Value.Y
                );
            }

            return position;
        }

        private static Vector2 GetAnchorOffset(Vector2 size, Anchor anchor)
        {
            return anchor switch
            {
                Anchor.TopLeft => Vector2.Zero,
                Anchor.TopCenter => new Vector2(size.X * 0.5f, 0),
                Anchor.TopRight => new Vector2(size.X, 0),
                Anchor.MiddleLeft => new Vector2(0, size.Y * 0.5f),
                Anchor.MiddleCenter => new Vector2(size.X * 0.5f, size.Y * 0.5f),
                Anchor.MiddleRight => new Vector2(size.X, size.Y * 0.5f),
                Anchor.BottomLeft => new Vector2(0, size.Y),
                Anchor.BottomCenter => new Vector2(size.X * 0.5f, size.Y),
                Anchor.BottomRight => new Vector2(size.X, size.Y),
                _ => Vector2.Zero
            };
        }

        public static void DebugLayout(IUIComponent component, string name = "Component")
        {
            var layout = component.Layout;
            var position = CalculatePosition(component);
            var size = CalculateSize(component);
            var bounds = new Rectangle((int)position.X, (int)position.Y, (int)size.X, (int)size.Y);

            System.Console.WriteLine($"{name}:");
            System.Console.WriteLine($"  Position: {position}");
            System.Console.WriteLine($"  Size: {size}");
            System.Console.WriteLine($"  Bounds: {bounds}");
            System.Console.WriteLine($"  AbsolutePosition: {layout.AbsolutePosition}");
            System.Console.WriteLine($"  RelativePosition: {layout.RelativePosition}");
            System.Console.WriteLine($"  Anchor: {layout.Anchor}");

            if (component.Parent != null)
            {
                var parentBounds = component.Parent.ContentBounds;
                System.Console.WriteLine($"  Parent Bounds: {parentBounds}");

                if (layout.RelativePosition.HasValue)
                {
                    var relativeOffset = new Vector2(
                        parentBounds.Width * layout.RelativePosition.Value.X,
                        parentBounds.Height * layout.RelativePosition.Value.Y
                    );
                    System.Console.WriteLine($"  Relative Offset (pixels): {relativeOffset}");
                }
            }

            var pivotOffset = GetAnchorOffset(size, layout.Anchor);
            System.Console.WriteLine($"  Pivot Offset: {pivotOffset}");
            System.Console.WriteLine();
        }
    }
}
