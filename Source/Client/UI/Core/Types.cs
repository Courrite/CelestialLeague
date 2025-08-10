using Microsoft.Xna.Framework;

namespace CelestialLeague.Client.UI.Core
{
    public class LayoutInfo
    {
        public Vector2? AbsolutePosition { get; set; }
        public Vector2? RelativePosition { get; set; }
        public Vector2? Offset { get; set; }

        public Vector2? AbsoluteSize { get; set; }
        public Vector2? RelativeSize { get; set; }
        public Vector2? MinSize { get; set; }
        public Vector2? MaxSize { get; set; }

        public Anchor Anchor { get; set; } = Anchor.TopLeft;
        public Vector2 Pivot { get; set; } = Vector2.Zero;

        public Thickness Margin { get; set; }
        public Thickness Padding { get; set; }

        public bool FillParent { get; set; }
        public LayoutDirection Direction { get; set; } = LayoutDirection.None;
    }

    public struct Thickness
    {
        public float Left, Top, Right, Bottom;

        public Thickness(float all) : this(all, all, all, all) { }
        public Thickness(float horizontal, float vertical) : this(horizontal, vertical, horizontal, vertical) { }
        public Thickness(float left, float top, float right, float bottom)
        {
            Left = left; Top = top; Right = right; Bottom = bottom;
        }
    }

    public enum Anchor
    {
        TopLeft, TopCenter, TopRight,
        MiddleLeft, MiddleCenter, MiddleRight,
        BottomLeft, BottomCenter, BottomRight
    }

    public enum LayoutDirection
    {
        None, Horizontal, Vertical, Grid
    }

}