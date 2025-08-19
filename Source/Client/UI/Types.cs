using Microsoft.Xna.Framework;

namespace CelestialLeague.Client.UI.Types
{
    public class LayoutInfo
    {
        // this is where u define the properties
        public DimensionUnit2 Position { get; set; } = new DimensionUnit2(0, 0, 0, 0);
        public DimensionUnit2 Size { get; set; } = new DimensionUnit2(0, 0, 0, 0);

        // to avoid confusion, absolute properties are now information on where and how to render stuff
        // anchor will offset the bounds set by absolute size placed around said location
        public Vector2 AbsolutePosition { get; internal set; } = Vector2.Zero;
        public Vector2 AbsoluteSize { get; internal set; } = Vector2.Zero;
        public Vector2 Anchor { get; set; } = Vector2.Zero;
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

    public struct DimensionUnit2
    {
        public DimensionUnit X;
        public DimensionUnit Y;

        public DimensionUnit2(DimensionUnit x, DimensionUnit y)
        {
            X = x;
            Y = y;
        }

        public DimensionUnit2(float XScale, int XOffset, float YScale, int YOffset)
        {
            X = new DimensionUnit(XScale, XOffset);
            Y = new DimensionUnit(YScale, YOffset);
        }

        public static DimensionUnit2 Zero => new(0, 0, 0, 0);

        public Vector2 Resolve(Vector2 parentSize)
        {
            return new Vector2(
                X.Resolve(parentSize.X),
                Y.Resolve(parentSize.Y)
            );
        }
    }

    public struct DimensionUnit
    {
        public float Scale = 0; // percentage of the parent
        public int Offset = 0; // pixel offset

        public DimensionUnit(float scale, int offset)
        {
            Scale = scale;
            Offset = offset;
        }

        public DimensionUnit(float scale) : this(scale, 0) { }
        public DimensionUnit(int offset) : this(0f, offset) { }

        public float Resolve(float parentSize)
        {
            float scaleValue = Scale;
            int offsetValue = Offset;
            return parentSize * scaleValue + offsetValue;
        }

        public bool IsSet => Scale != 0 || Offset != 0;
    }

    public struct SpriteSheetConfig(int frameWidth, int frameHeight, int framesPerRow, int totalFrames, float frameDuration = 0.1f, Point startOffset = default)
    {
        public int FrameWidth = frameWidth;
        public int FrameHeight = frameHeight;
        public int FramesPerRow = framesPerRow;
        public int TotalFrames = totalFrames;
        public float FrameDuration = frameDuration; // duration per frame in seconds
        public Point StartOffset = startOffset == default ? Point.Zero : startOffset; // starting position in the texture (default 0,0)

        public static SpriteSheetConfig FromGrid(int frameWidth, int frameHeight, int framesPerRow, int totalFrames, float frameDuration = 0.1f)
        {
            return new SpriteSheetConfig(frameWidth, frameHeight, framesPerRow, totalFrames, frameDuration);
        }

        public static SpriteSheetConfig FromRow(int frameWidth, int frameHeight, int totalFrames, float frameDuration = 0.1f)
        {
            return new SpriteSheetConfig(frameWidth, frameHeight, totalFrames, totalFrames, frameDuration);
        }
    }

    public enum ImageType
    {
        Static,
        SpriteSheet
    }

    public enum ScaleType
    {
        Fit, // scale proportionally to fit within bounds
        Stretch, // stretch to fill the entire bounds
        Crop, // scale proportionally to fill bounds, cropping excess
        Tile // repeat the image to fill the bounds
    }

    public enum ConstraintAxis
    {
        None,
        Width,
        Height
    }

    public enum BorderStyle
    {
        None,
        Solid,
        Dashed,
        Dotted
    }

    public enum LayoutDirection
    {
        None, Horizontal, Vertical, Grid
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
        Center,
        Bottom
    }

    public enum TextStyle
    {
        Normal,
        Underline,
        Strikethrough,
        AllCaps
    }

    public enum SortOrder
    {
        Name,
        LayoutOrder
    }
}