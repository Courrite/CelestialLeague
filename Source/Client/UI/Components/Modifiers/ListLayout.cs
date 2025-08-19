using System;
using System.Linq;
using CelestialLeague.Client.UI.Types;
using Microsoft.Xna.Framework;

namespace CelestialLeague.Client.UI.Components
{
    public class ListLayout : UIComponent
    {
        public LayoutDirection Direction { get; set; } = LayoutDirection.Vertical;
        public Vector2 Padding { get; set; } = Vector2.Zero;
        public HorizontalAlignment HorizontalAlignment { get; set; } = HorizontalAlignment.Left;
        public VerticalAlignment VerticalAlignment { get; set; } = VerticalAlignment.Top;
        public bool Wrap { get; set; } = false;

        public ListLayout()
        {
        }

        protected override void UpdateSelf(InterfaceManager ui)
        {
            if (Parent == null) return;

            Rectangle parentBounds = Parent.Bounds;
            Vector2 currentPosition = Vector2.Zero;
            float currentLineHeight = 0;
            float currentLineWidth = 0;

            var siblings = Parent.Children.Where(c => c != this).ToList();

            foreach (var child in siblings)
            {
                Vector2 childSize = child.Layout.AbsoluteSize;
                
                float horizontalOffset = 0;
                float verticalOffset = 0;
                
                if (Direction == LayoutDirection.Vertical)
                {
                    if (Wrap && currentPosition.Y + childSize.Y > parentBounds.Height && currentPosition.Y != 0)
                    {
                        currentPosition.Y = 0;
                        currentPosition.X += currentLineWidth + Padding.X;
                        currentLineWidth = 0;
                    }

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

                    child.Layout.Position = new DimensionUnit2(new DimensionUnit((int)(currentPosition.X + horizontalOffset)), new DimensionUnit((int)currentPosition.Y));
                    child.Layout.Anchor = Vector2.Zero;
                    currentPosition.Y += childSize.Y / 2 + Padding.Y;
                    currentLineWidth = Math.Max(currentLineWidth, childSize.X);
                }
                else if (Direction == LayoutDirection.Horizontal)
                {
                    if (Wrap && currentPosition.X + childSize.X > parentBounds.Width && currentPosition.X != 0)
                    {
                        currentPosition.X = 0;
                        currentPosition.Y += currentLineHeight + Padding.Y;
                        currentLineHeight = 0;
                    }

                    switch (VerticalAlignment)
                    {
                        case VerticalAlignment.Center:
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

                    child.Layout.Position = new DimensionUnit2(new DimensionUnit((int)currentPosition.X), new DimensionUnit((int)(currentPosition.Y + verticalOffset)));
                    child.Layout.Anchor = Vector2.Zero;
                    currentPosition.X += childSize.X / 2 + Padding.X;
                    currentLineHeight = Math.Max(currentLineHeight, childSize.Y);
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
