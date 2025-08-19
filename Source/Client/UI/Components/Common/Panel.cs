using Celeste.Mod;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using CelestialLeague.Client.Motion;
using Monocle;
using CelestialLeague.Client.UI;
using System.Linq;
using System;
using CelestialLeague.Client.UI.Types;

namespace CelestialLeague.Client.UI.Components
{
    public class Panel : UIComponent
    {
        public Color BackgroundColor { get; set; } = Color.DarkGray;
        public Borders PanelBorders { get; set; } = new Borders(0);
        public bool ClipContent { get; set; } = false;

        public Panel()
        {
            Layout.Size = new DimensionUnit2(0.1f, 0, 0.1f, 0);
        }

        protected override void UpdateSelf(InterfaceManager ui)
        {
            // this is a container so it doesnt need updates
        }

        protected override void RenderSelf(InterfaceManager ui)
        {
            var bounds = new Rectangle(
                (int)Layout.AbsolutePosition.X,
                (int)Layout.AbsolutePosition.Y,
                (int)Layout.AbsoluteSize.X,
                (int)Layout.AbsoluteSize.Y
            );

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
                    Rectangle dot = new Rectangle(bounds.X + (bounds.Width - dotSize) / 2, y, bounds.Width, dotSize);
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
            var bounds = Bounds;
            if (bounds.Width <= 0 || bounds.Height <= 0) return;

            ui.SpriteBatch.End();

            var oldScissor = ui.GraphicsDevice.ScissorRectangle;

            var newScissor = oldScissor == Rectangle.Empty ? bounds : Rectangle.Intersect(oldScissor, bounds);
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
    }
}
