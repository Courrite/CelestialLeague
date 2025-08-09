using Celeste.Mod;
using Microsoft.Xna.Framework;

public class Panel : UIComponent
{
    public Color BackgroundColor { get; set; } = Color.DarkGray;
    public bool DrawBackground { get; set; } = true;
    public bool DrawBorder { get; set; } = true;

    public Panel()
    {
        Layout.RelativeSize = new Vector2(1, 1);
    }

    protected override void UpdateSelf(InterfaceManager ui)
    {
        // this is just a container bruh
    }

    protected override void RenderSelf(InterfaceManager ui)
    {
        var bounds = GetWorldBounds();
        
        if (DrawBackground)
        {
            ui.DrawRectangle(bounds, BackgroundColor);
        }
        
        if (DrawBorder)
        {
            ui.DrawRectangleOutline(bounds, Color.Black, 1);
        }
    }
}