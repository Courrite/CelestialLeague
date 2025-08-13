using Microsoft.Xna.Framework;

namespace CelestialLeague.Client.UI.Core
{
    public static class LayoutHelper
    {
        public static Vector2 CalculatePosition(IUIComponent component)
        {
            var layout = component.Layout;

            Vector2 basePosition = CalculateBasePosition(component);

            if (layout.Offset.HasValue)
                basePosition += layout.Offset.Value;

            basePosition += new Vector2(layout.Margin.Left, layout.Margin.Top);

            var componentSize = CalculateSize(component);
            Vector2 pivotOffset = new Vector2(
                componentSize.X * layout.Pivot.X,
                componentSize.Y * layout.Pivot.Y
            );
            basePosition -= pivotOffset;

            return basePosition;
        }

        public static Vector2 CalculateSize(IUIComponent component)
        {
            var layout = component.Layout;
            Vector2 size = new Vector2(100, 50);

            if (layout.AbsoluteSize.HasValue)
            {
                size = layout.AbsoluteSize.Value;
            }
            else if (layout.RelativeSize.HasValue && component.Parent != null)
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
            Vector2 parentOrigin = new Vector2(parentBounds.X, parentBounds.Y);
            Vector2 anchorPosition = GetAnchorOffset(parentBounds, layout.Anchor);

            if (layout.RelativePosition.HasValue)
            {
                anchorPosition += new Vector2(
                    parentBounds.Width * layout.RelativePosition.Value.X,
                    parentBounds.Height * layout.RelativePosition.Value.Y
                );
            }

            return parentOrigin + anchorPosition;
        }

        private static Vector2 GetAnchorOffset(Rectangle parentBounds, Anchor anchor)
        {
            return anchor switch
            {
                Anchor.TopLeft => Vector2.Zero,
                Anchor.TopCenter => new Vector2(parentBounds.Width * 0.5f, 0),
                Anchor.TopRight => new Vector2(parentBounds.Width, 0),
                Anchor.MiddleLeft => new Vector2(0, parentBounds.Height * 0.5f),
                Anchor.MiddleCenter => new Vector2(parentBounds.Width * 0.5f, parentBounds.Height * 0.5f),
                Anchor.MiddleRight => new Vector2(parentBounds.Width, parentBounds.Height * 0.5f),
                Anchor.BottomLeft => new Vector2(0, parentBounds.Height),
                Anchor.BottomCenter => new Vector2(parentBounds.Width * 0.5f, parentBounds.Height),
                Anchor.BottomRight => new Vector2(parentBounds.Width, parentBounds.Height),
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
            System.Console.WriteLine($"  RelativePosition: {layout.RelativePosition} (as percentage 0-1)");
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
            System.Console.WriteLine();
        }
    }
}