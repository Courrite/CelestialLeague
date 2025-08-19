#nullable enable

using CelestialLeague.Client.UI.Types;
using Microsoft.Xna.Framework;
using System;

namespace CelestialLeague.Client.UI.Components
{
    public class AspectRatio : UIComponent
    {
        public float Ratio { get; set; } = 1.0f;
        public ConstraintAxis ConstrainAxis { get; set; } = ConstraintAxis.None;

        public AspectRatio()
        {
            IsVisible = false;
            CanReceiveFocus = false;
        }

        protected override void RenderSelf(InterfaceManager ui)
        {
            // no rendering for this component
        }

        protected override void UpdateSelf(InterfaceManager ui)
        {
            if (Parent == null || ConstrainAxis == ConstraintAxis.None)
            {
                return;
            }

            Vector2 parentSize = Parent.Layout.Size.Resolve(Parent.Parent?.Layout.AbsoluteSize ?? new Vector2(ui.GraphicsDevice.Viewport.Width, ui.GraphicsDevice.Viewport.Height));

            if (ConstrainAxis == ConstraintAxis.Width)
            {
                float newHeight = parentSize.X / Ratio;
                if (Math.Abs(Parent.Layout.Size.Y.Scale * (Parent.Parent?.Bounds.Height ?? 0) - newHeight) > 1f)
                {
                    Parent.Layout.Size = new DimensionUnit2(Parent.Layout.Size.X, new DimensionUnit(0, (int)newHeight));
                    Parent.InvalidateLayout();
                }
            }
            else if (ConstrainAxis == ConstraintAxis.Height)
            {
                float newWidth = parentSize.Y * Ratio;
                if (Math.Abs(Parent.Layout.Size.X.Scale * (Parent.Parent?.Bounds.Width ?? 0) - newWidth) > 1f)
                {
                    Parent.Layout.Size = new DimensionUnit2(new DimensionUnit(0, (int)newWidth), Parent.Layout.Size.Y);
                    Parent.InvalidateLayout();
                }
            }
        }
    }
}
