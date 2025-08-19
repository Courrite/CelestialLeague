using System;
using System.Linq;
using Microsoft.Xna.Framework;

namespace CelestialLeague.Client.UI.Components
{
    public class ListLayout : UIComponent
    {
        public LayoutDirection Direction { get; set; } = LayoutDirection.Vertical;
        public Vector2 Padding { get; set; } = Vector2.Zero;
        public HorizontalAlignment HorizontalAlignment { get; set; } = HorizontalAlignment.Left;
        public VerticalAlignment VerticalAlignment { get; set; } = VerticalAlignment.Top;

        public ListLayout()
        {
        }

        protected override void UpdateSelf(InterfaceManager ui)
        {
            if (Parent == null) return;

            Rectangle parentBounds = Parent.Bounds;
            Vector2 currentPosition = Vector2.Zero;

            var siblings = Parent.Children.Where(c => c != this).ToList();

            foreach (var child in siblings)
            {
                Vector2 childSize = child.Layout.AbsoluteSize;

                float horizontalOffset = 0;
                float verticalOffset = 0;

                if (Direction == LayoutDirection.Vertical)
                {
                    switch (HorizontalAlignment)
                    {
                        case HorizontalAlignment.Center:
                            horizontalOffset = (parentBounds.Width - childSize.X) / 2f;
                            break;
                        case HorizontalAlignment.Right:
                            horizontalOffset = parentBounds.Width - childSize.X;
                            break;
                        case HorizontalAlignment.Left:
                        default:
                            horizontalOffset = 0;
                            break;
                    }

                    child.Layout.Position = new DimensionUnit2(new DimensionUnit((int)horizontalOffset), new DimensionUnit((int)currentPosition.Y));
                    child.Layout.Anchor = Vector2.Zero;

                    currentPosition.Y += childSize.Y / 2+ Padding.Y;
                }
                else if (Direction == LayoutDirection.Horizontal)
                {
                    switch (VerticalAlignment)
                    {
                        case VerticalAlignment.Middle:
                            verticalOffset = (parentBounds.Height - childSize.Y) / 2f;
                            break;
                        case VerticalAlignment.Bottom:
                            verticalOffset = parentBounds.Height - childSize.Y;
                            break;
                        case VerticalAlignment.Top:
                        default:
                            verticalOffset = 0;
                            break;
                    }

                    child.Layout.Position = new DimensionUnit2(new DimensionUnit((int)currentPosition.X), new DimensionUnit((int)verticalOffset));
                    child.Layout.Anchor = Vector2.Zero;

                    currentPosition.X += childSize.X / 2 + Padding.X;
                }

                child.InvalidateLayout();
            }
        }

        protected override void RenderSelf(InterfaceManager ui)
        {
            // the component doesnt have a visual representation
        }
    }
}