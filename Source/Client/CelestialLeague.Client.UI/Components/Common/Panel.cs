using Celeste.Mod;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using CelestialLeague.Client.Motion;
using Monocle;
using CelestialLeague.Client.UI;
using System.Linq;
using System;

namespace CelestialLeague.Client.UI.Components
{
    public class Panel : UIComponent
    {
        public Color BackgroundColor { get; set; } = Color.DarkGray;
        public Borders PanelBorders { get; set; } = new Borders(0);
        public bool ClipContent { get; set; } = false;

        public Panel()
        {
            Layout.RelativeSize = new Vector2(0.1f, 0.1f);
        }

        protected override void UpdateSelf(InterfaceManager ui)
        {
            // this is a container so it doesnt need updates
        }

        protected override void RenderSelf(InterfaceManager ui)
        {
            var bounds = GetWorldBounds();

            if (BackgroundColor.A > 0)
            {
                ui.DrawRectangle(bounds, BackgroundColor);
            }

            RenderBorders(ui, bounds);
        }

        private void RenderBorders(InterfaceManager ui, Rectangle bounds)
        {
            if (!PanelBorders.Enabled) return;
            if (!PanelBorders.HasAnyBorder) return;

            if (PanelBorders.HasTop)
            {
                Rectangle topBorder = new Rectangle(
                    bounds.X,
                    bounds.Y - PanelBorders.Top.Width,
                    bounds.Width,
                    PanelBorders.Top.Width
                );
                RenderBorderSegment(ui, topBorder, PanelBorders.Top.Color, PanelBorders.Top.Style);
            }

            if (PanelBorders.HasRight)
            {
                Rectangle rightBorder = new Rectangle(
                    bounds.X + bounds.Width,
                    bounds.Y - PanelBorders.Top.Width,
                    PanelBorders.Right.Width,
                    bounds.Height + PanelBorders.TotalVertical
                );
                RenderBorderSegment(ui, rightBorder, PanelBorders.Right.Color, PanelBorders.Right.Style);
            }

            if (PanelBorders.HasBottom)
            {
                Rectangle bottomBorder = new Rectangle(
                    bounds.X,
                    bounds.Y + bounds.Height,
                    bounds.Width,
                    PanelBorders.Bottom.Width
                );
                RenderBorderSegment(ui, bottomBorder, PanelBorders.Bottom.Color, PanelBorders.Bottom.Style);
            }

            if (PanelBorders.HasLeft)
            {
                Rectangle leftBorder = new Rectangle(
                    bounds.X - PanelBorders.Left.Width,
                    bounds.Y - PanelBorders.Top.Width,
                    PanelBorders.Left.Width,
                    bounds.Height + PanelBorders.TotalVertical
                );
                RenderBorderSegment(ui, leftBorder, PanelBorders.Left.Color, PanelBorders.Left.Style);
            }
        }

        private void RenderBorderSegment(InterfaceManager ui, Rectangle bounds, Color color, BorderStyle style)
        {
            switch (style)
            {
                case BorderStyle.Solid:
                    ui.DrawRectangle(bounds, color);
                    break;
                case BorderStyle.Dashed:
                    RenderDashedBorder(ui, bounds, color);
                    break;
                case BorderStyle.Dotted:
                    RenderDottedBorder(ui, bounds, color);
                    break;
                case BorderStyle.None:
                default:
                    break;
            }
        }

        private void RenderDashedBorder(InterfaceManager ui, Rectangle bounds, Color color)
        {
            const int dashLength = 8;
            const int gapLength = 4;
            const int segmentLength = dashLength + gapLength;

            if (bounds.Width > bounds.Height) // Horizontal border
            {
                for (int x = bounds.X; x < bounds.X + bounds.Width; x += segmentLength)
                {
                    int width = Math.Min(dashLength, bounds.X + bounds.Width - x);
                    if (width > 0)
                    {
                        Rectangle dash = new Rectangle(x, bounds.Y, width, bounds.Height);
                        ui.DrawRectangle(dash, color);
                    }
                }
            }
            else // Vertical border
            {
                for (int y = bounds.Y; y < bounds.Y + bounds.Height; y += segmentLength)
                {
                    int height = Math.Min(dashLength, bounds.Y + bounds.Height - y);
                    if (height > 0)
                    {
                        Rectangle dash = new Rectangle(bounds.X, y, bounds.Width, height);
                        ui.DrawRectangle(dash, color);
                    }
                }
            }
        }

        private void RenderDottedBorder(InterfaceManager ui, Rectangle bounds, Color color)
        {
            const int dotSize = 2;
            const int spacing = 4;

            if (bounds.Width > bounds.Height) // Horizontal border
            {
                for (int x = bounds.X; x < bounds.X + bounds.Width; x += spacing)
                {
                    Rectangle dot = new Rectangle(x, bounds.Y + (bounds.Height - dotSize) / 2, dotSize, dotSize);
                    ui.DrawRectangle(dot, color);
                }
            }
            else // Vertical border
            {
                for (int y = bounds.Y; y < bounds.Y + bounds.Height; y += spacing)
                {
                    Rectangle dot = new Rectangle(bounds.X + (bounds.Width - dotSize) / 2, y, dotSize, dotSize);
                    ui.DrawRectangle(dot, color);
                }
            }
        }

        public override void Render(InterfaceManager ui)
        {
            if (!IsVisible) return;

            RenderSelf(ui);

            if (ClipContent)
            {
                RenderChildrenWithClipping(ui);
            }
            else
            {
                foreach (var child in Children.Where(c => c.IsVisible).OrderBy(c => c.RenderOrder))
                {
                    child.Render(ui);
                }
            }
        }

        private void RenderChildrenWithClipping(InterfaceManager ui)
        {
            var contentBounds = ContentBounds;
            if (contentBounds.Width <= 0 || contentBounds.Height <= 0) return;

            ui.SpriteBatch.End();

            var oldScissor = ui.GraphicsDevice.ScissorRectangle;

            var newScissor = oldScissor == Rectangle.Empty ? contentBounds : Rectangle.Intersect(oldScissor, contentBounds);
            ui.GraphicsDevice.ScissorRectangle = newScissor;

            ui.SpriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                SamplerState.LinearClamp,
                DepthStencilState.None,
                new RasterizerState { ScissorTestEnable = true, CullMode = CullMode.None },
                null,
                Matrix.Identity
            );

            foreach (var child in Children.Where(c => c.IsVisible).OrderBy(c => c.RenderOrder))
            {
                child.Render(ui);
            }

            ui.SpriteBatch.End();
            ui.GraphicsDevice.ScissorRectangle = oldScissor;

            ui.SpriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                SamplerState.LinearClamp,
                DepthStencilState.None,
                RasterizerState.CullNone,
                null,
                Matrix.Identity
            );
        }

        public override void InvalidateLayout()
        {
            base.InvalidateLayout();
            // no render target here anymore
        }

        public class Border
        {
            public int Width { get; set; }
            public Color Color { get; set; } = Color.Black;
            public BorderStyle Style { get; set; } = BorderStyle.Solid;

            public Border() { }

            public Border(int width, Color color = default, BorderStyle style = BorderStyle.Solid)
            {
                Width = width;
                Color = (color == default) ? Color.Black : color;
                Style = style;
            }

            public bool IsVisible => Width > 0 && Color.A > 0;
        }

        public class Borders
        {
            public bool Enabled = true;

            public Border Top { get; set; } = new Border();
            public Border Right { get; set; } = new Border();
            public Border Bottom { get; set; } = new Border();
            public Border Left { get; set; } = new Border();

            public Borders() { }

            public Borders(int width) : this(width, width, width, width) { }

            public Borders(int vertical, int horizontal) : this(vertical, horizontal, vertical, horizontal) { }

            public Borders(int top, int right, int bottom, int left)
            {
                Top = new Border(top);
                Right = new Border(right);
                Bottom = new Border(bottom);
                Left = new Border(left);
            }

            public bool HasTop => Top.IsVisible;
            public bool HasRight => Right.IsVisible;
            public bool HasBottom => Bottom.IsVisible;
            public bool HasLeft => Left.IsVisible;
            public bool HasAnyBorder => HasTop || HasRight || HasBottom || HasLeft;

            public int TotalHorizontal => Left.Width + Right.Width;
            public int TotalVertical => Top.Width + Bottom.Width;
        }

        public enum BorderStyle
        {
            None,
            Solid,
            Dashed,
            Dotted
        }
    }
}
