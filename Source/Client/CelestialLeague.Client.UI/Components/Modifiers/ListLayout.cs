using CelestialLeague.Client.UI;
using Microsoft.Xna.Framework;
using System;
using System.Linq;

namespace CelestialLeague.Client.UI.Components
{
    public class ListLayout : UIComponent
    {
        public FillDirection FillDirection { get; set; } = FillDirection.Vertical;
        public HorizontalAlignment HorizontalAlignment { get; set; } = HorizontalAlignment.Left;
        public VerticalAlignment VerticalAlignment { get; set; } = VerticalAlignment.Top;
        public SortOrder SortOrder { get; set; } = SortOrder.Name;
        public Vector2 Padding { get; set; } = Vector2.Zero;

        private int _spacing = 0;
        public int Spacing
        {
            get => _spacing;
            set
            {
                if (_spacing != value)
                {
                    _spacing = value;
                    InvalidateLayout();
                }
            }
        }

        public override int RenderOrder => -1; // layout constraints should update before regular components

        public ListLayout()
        {
            CanReceiveFocus = false;
        }

        protected override void UpdateSelf(InterfaceManager ui)
        {
            if (Parent != null)
            {
                Layout.RelativePosition = Vector2.Zero;
                Layout.RelativeSize = new Vector2(1f, 1f);

                ArrangeSiblings();
            }
        }

        public override void Render(InterfaceManager ui)
        {
            if (!Parent.IsVisible) return;
        }

        protected override void RenderSelf(InterfaceManager ui)
        {
            // required to implement this
            // wont let otherwise
        }

        private void ArrangeSiblings()
        {
            if (Parent == null) return;

            var siblings = GetSortedSiblings();
            var contentBounds = Parent.ContentBounds;

            if (siblings.Count == 0) return;

            Vector2 currentPosition = new Vector2(contentBounds.X, contentBounds.Y) + Padding;

            // arrange each sibling
            foreach (var sibling in siblings)
            {
                if (sibling == this) continue;

                var childSize = CalculateChildSize(sibling, contentBounds);
                Vector2 childPosition = currentPosition;

                ApplyAlignment(ref childPosition, childSize, contentBounds);

                sibling.Layout.AbsolutePosition = childPosition;
                sibling.InvalidateLayout();

                AdvancePosition(ref currentPosition, childSize);
            }
        }

        private void ApplyAlignment(ref Vector2 childPosition, Vector2 childSize, Rectangle contentBounds)
        {
            if (FillDirection == FillDirection.Vertical)
            {
                switch (HorizontalAlignment)
                {
                    case HorizontalAlignment.Center:
                        childPosition.X += (contentBounds.Width - Padding.X * 2 - childSize.X) * 0.5f;
                        break;
                    case HorizontalAlignment.Right:
                        childPosition.X += contentBounds.Width - Padding.X * 2 - childSize.X;
                        break;
                }
            }
            else
            {
                switch (VerticalAlignment)
                {
                    case VerticalAlignment.Middle:
                        childPosition.Y += (contentBounds.Height - Padding.Y * 2 - childSize.Y) * 0.5f;
                        break;
                    case VerticalAlignment.Bottom:
                        childPosition.Y += contentBounds.Height - Padding.Y * 2 - childSize.Y;
                        break;
                }
            }
        }

        private void AdvancePosition(ref Vector2 currentPosition, Vector2 childSize)
        {
            if (FillDirection == FillDirection.Vertical)
            {
                currentPosition.Y += childSize.Y + Spacing;
            }
            else
            {
                currentPosition.X += childSize.X + Spacing;
            }
        }

        private System.Collections.Generic.List<IUIComponent> GetSortedSiblings()
        {
            if (Parent == null) return new System.Collections.Generic.List<IUIComponent>();

            var siblings = Parent.Children.Where(c => c.IsVisible).ToList();

            return SortOrder switch
            {
                SortOrder.Name => siblings.OrderBy(c => c.Name).ToList(),
                SortOrder.LayoutOrder => siblings.OrderBy(c => c.RenderOrder).ToList(),
                _ => siblings.ToList()
            };
        }

        private Vector2 CalculateChildSize(IUIComponent child, Rectangle parentBounds)
        {
            var childSize = LayoutHelper.CalculateSize(child);
            var availableSpace = new Vector2(
                parentBounds.Width - Padding.X * 2,
                parentBounds.Height - Padding.Y * 2
            );

            // fill available space in the cross-axis if no size specified
            if (FillDirection == FillDirection.Vertical && child.Layout.RelativeSize?.X == null && child.Layout.AbsoluteSize?.X == null)
            {
                childSize.X = availableSpace.X;
            }
            else if (FillDirection == FillDirection.Horizontal && child.Layout.RelativeSize?.Y == null && child.Layout.AbsoluteSize?.Y == null)
            {
                childSize.Y = availableSpace.Y;
            }

            return childSize;
        }

        public Vector2 GetContentSize()
        {
            if (Parent == null) return Vector2.Zero;

            var siblings = GetSortedSiblings().Where(c => c != this).ToList();
            if (siblings.Count == 0) return Vector2.Zero;

            Vector2 totalSize = Padding * 2;
            Vector2 maxCrossAxisSize = Vector2.Zero;

            foreach (var sibling in siblings)
            {
                var childSize = CalculateChildSize(sibling, Parent.ContentBounds);

                if (FillDirection == FillDirection.Vertical)
                {
                    totalSize.Y += childSize.Y;
                    maxCrossAxisSize.X = Math.Max(maxCrossAxisSize.X, childSize.X);
                }
                else
                {
                    totalSize.X += childSize.X;
                    maxCrossAxisSize.Y = Math.Max(maxCrossAxisSize.Y, childSize.Y);
                }
            }

            // add spacing between items (count - 1 gaps)
            if (siblings.Count > 1)
            {
                if (FillDirection == FillDirection.Vertical)
                {
                    totalSize.Y += Spacing * (siblings.Count - 1);
                }
                else
                {
                    totalSize.X += Spacing * (siblings.Count - 1);
                }
            }

            // set the cross-axis size
            if (FillDirection == FillDirection.Vertical)
            {
                totalSize.X = maxCrossAxisSize.X + Padding.X * 2;
            }
            else
            {
                totalSize.Y = maxCrossAxisSize.Y + Padding.Y * 2;
            }

            return totalSize;
        }
    }

    public enum FillDirection
    {
        Vertical,
        Horizontal
    }

    public enum HorizontalAlignment
    {
        Left,
        Center,
        Right
    }

    public enum VerticalAlignment
    {
        Top,
        Middle,
        Bottom
    }

    public enum SortOrder
    {
        Name,
        LayoutOrder
    }
}