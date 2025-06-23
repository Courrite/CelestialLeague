using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using CelestialLeague.Client.UI.Core;
using System;
using Monocle;

namespace CelestialLeague.Client.UI.Components
{
    public enum PanelLayout
    {
        None, // manual positioning
        Vertical, // stack children vertically
        Horizontal, // stack children horizontally
        Grid // arrange in grid
    }

    public class Panel : UIComponent
    {
        private Vector2 _scrollOffset = Vector2.Zero;
        public PanelLayout Layout { get; set; } = PanelLayout.None;
        public Vector2 Padding { get; set; } = new Vector2(10, 10);
        public float Spacing { get; set; } = 5f;
        public int GridColumns { get; set; } = 2;
        public bool AutoSize { get; set; } = false;
        public bool ClipChildren { get; set; } = false;

        // scroll support
        public bool IsScrollable { get; set; } = false;
        public Vector2 ScrollOffset
        {
            get => _scrollOffset;
            set => _scrollOffset = value;
        }
        public Vector2 ContentSize { get; private set; } = Vector2.Zero;

        public Panel(string id = null) : base(id)
        {
            CanReceiveInput = false; // panels typically don't need input unless scrollable
            BackgroundColor = Color.Transparent;
        }

        public Panel(PanelLayout layout, string id = null) : this(id)
        {
            Layout = layout;
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();

            if (Layout != PanelLayout.None)
            {
                UpdateLayout();
            }

            if (IsScrollable && IsHovered)
            {
                HandleScrollInput();
            }
        }

        protected override void OnDraw(SpriteBatch spriteBatch)
        {
            if (ClipChildren)
            {
                // TODO: implement clipping using RasterizerState
                // for now, draw normally
                base.OnDraw(spriteBatch);
            }
            else
            {
                base.OnDraw(spriteBatch);
            }
        }

        private void UpdateLayout()
        {
            if (Children.Count == 0) return;

            var contentArea = new Rectangle(
                (int)Padding.X,
                (int)Padding.Y,
                (int)(Size.X - Padding.X * 2),
                (int)(Size.Y - Padding.Y * 2)
            );

            switch (Layout)
            {
                case PanelLayout.Vertical:
                    LayoutVertical(contentArea);
                    break;
                case PanelLayout.Horizontal:
                    LayoutHorizontal(contentArea);
                    break;
                case PanelLayout.Grid:
                    LayoutGrid(contentArea);
                    break;
            }

            if (AutoSize)
            {
                UpdateAutoSize();
            }
        }

        private void LayoutVertical(Rectangle contentArea)
        {
            float currentY = contentArea.Y - ScrollOffset.Y;
            float maxWidth = 0;

            foreach (var child in Children)
            {
                if (!child.IsVisible) continue;

                child.Position = new Vector2(contentArea.X - ScrollOffset.X, currentY);
                currentY += child.Size.Y + Spacing;
                maxWidth = Math.Max(maxWidth, child.Size.X);
            }

            ContentSize = new Vector2(maxWidth, currentY - contentArea.Y + ScrollOffset.Y);
        }

        private void LayoutHorizontal(Rectangle contentArea)
        {
            float currentX = contentArea.X - ScrollOffset.X;
            float maxHeight = 0;

            foreach (var child in Children)
            {
                if (!child.IsVisible) continue;

                child.Position = new Vector2(currentX, contentArea.Y - ScrollOffset.Y);
                currentX += child.Size.X + Spacing;
                maxHeight = Math.Max(maxHeight, child.Size.Y);
            }

            ContentSize = new Vector2(currentX - contentArea.X + ScrollOffset.X, maxHeight);
        }

        private void LayoutGrid(Rectangle contentArea)
        {
            if (GridColumns <= 0) GridColumns = 1;

            float cellWidth = (contentArea.Width - (GridColumns - 1) * Spacing) / GridColumns;
            int row = 0, col = 0;
            float maxRowHeight = 0;
            float totalHeight = 0;

            foreach (var child in Children)
            {
                if (!child.IsVisible) continue;

                float x = contentArea.X + col * (cellWidth + Spacing) - ScrollOffset.X;
                float y = contentArea.Y + totalHeight - ScrollOffset.Y;

                child.Position = new Vector2(x, y);
                maxRowHeight = Math.Max(maxRowHeight, child.Size.Y);

                col++;
                if (col >= GridColumns)
                {
                    col = 0;
                    row++;
                    totalHeight += maxRowHeight + Spacing;
                    maxRowHeight = 0;
                }
            }

            if (col > 0)
            {
                totalHeight += maxRowHeight;
            }

            ContentSize = new Vector2(contentArea.Width, totalHeight);
        }

        private void UpdateAutoSize()
        {
            if (Children.Count == 0) return;

            float minX = float.MaxValue, minY = float.MaxValue;
            float maxX = float.MinValue, maxY = float.MinValue;

            foreach (var child in Children)
            {
                if (!child.IsVisible) continue;

                minX = Math.Min(minX, child.Position.X);
                minY = Math.Min(minY, child.Position.Y);
                maxX = Math.Max(maxX, child.Position.X + child.Size.X);
                maxY = Math.Max(maxY, child.Position.Y + child.Size.Y);
            }

            if (minX != float.MaxValue)
            {
                Size = new Vector2(
                    maxX - minX + Padding.X * 2,
                    maxY - minY + Padding.Y * 2
                );
            }
        }

        private void HandleScrollInput()
        {
            var scrollDelta = MInput.Mouse.WheelDelta;
            if (scrollDelta != 0)
            {
                _scrollOffset.Y -= scrollDelta * 0.1f;
                _scrollOffset.Y = MathHelper.Clamp(_scrollOffset.Y, 0,
                    Math.Max(0, ContentSize.Y - Size.Y + Padding.Y * 2));
            }
        }

        // utility
        public void SetPadding(float horizontal, float vertical)
        {
            Padding = new Vector2(horizontal, vertical);
        }

        public void SetPadding(float padding)
        {
            Padding = new Vector2(padding, padding);
        }

        public void ScrollToTop()
        {
            ScrollOffset = Vector2.Zero;
        }

        public void ScrollToBottom()
        {
            _scrollOffset.Y = Math.Max(0, ContentSize.Y - Size.Y + Padding.Y * 2);
        }

        public void ScrollToChild(UIComponent child)
        {
            if (!Children.Contains(child)) return;

            var childBounds = child.Bounds;
            var panelBounds = Bounds;

            if (childBounds.Y < ScrollOffset.Y)
            {
                _scrollOffset.Y = childBounds.Y;
            }
            else if (childBounds.Bottom > ScrollOffset.Y + panelBounds.Height)
            {
                _scrollOffset.Y = childBounds.Bottom - panelBounds.Height;
            }
        }

        public bool IsChildVisible(UIComponent child)
        {
            if (!Children.Contains(child) || !child.IsVisible) return false;

            var childBounds = child.AbsoluteBounds;
            var panelBounds = AbsoluteBounds;

            return childBounds.Intersects(panelBounds);
        }
    }
}
