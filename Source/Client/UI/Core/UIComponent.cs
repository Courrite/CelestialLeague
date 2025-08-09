using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

public interface IUIComponent
{
    void Update(InterfaceManager ui);
    void Render(InterfaceManager ui);
    
    // Core properties
    bool IsVisible { get; set; }
    bool IsEnabled { get; set; }
    bool CanReceiveFocus { get; set; }
    bool HasFocus { get; set; }
    float BackgroundTransparency { get; set; }
    
    // Layout and rendering
    int RenderOrder { get; }
    LayoutInfo Layout { get; set; }
    Rectangle Bounds { get; }
    Rectangle ContentBounds { get; }
    
    // Hierarchy
    IUIComponent Parent { get; set; }
    IReadOnlyList<IUIComponent> Children { get; }
    void AddChild(IUIComponent child);
    void RemoveChild(IUIComponent child);
    void ClearChildren();
    T FindChild<T>(string name = null) where T : class, IUIComponent;
    
    // Events
    event Action<IUIComponent> OnClick;
    event Action<IUIComponent> OnPressed;
    event Action<IUIComponent> OnReleased;
    event Action<IUIComponent> OnMouseEnter;
    event Action<IUIComponent> OnMouseExit;
    event Action<IUIComponent> OnFocusGained;
    event Action<IUIComponent> OnFocusLost;
    event Action<IUIComponent, Keys> OnKeyPressed;
    
    // Selection navigation for console/gamepad
    IUIComponent NextSelection { get; set; }
    IUIComponent PreviousSelection { get; set; }
    
    // Transform and bounds
    Vector2 LocalToWorld(Vector2 localPosition);
    Vector2 WorldToLocal(Vector2 worldPosition);
    Rectangle GetWorldBounds();
    bool ContainsPoint(Vector2 worldPoint);
    
    // Event triggers for external exposure (for interfaceManager) 
    void FocusGained();
    void FocusLost();
    
    // Layout
    void InvalidateLayout();
    void UpdateLayout();
    
    // Identification
    string Name { get; set; }
    string Tag { get; set; }
}

public abstract class UIComponent : IUIComponent
{
    private readonly List<IUIComponent> children = new List<IUIComponent>();
    private IUIComponent parent;
    private bool layoutDirty = true;
    private Rectangle cachedBounds;
    
    private bool isHovered = false;
    private bool isPressed = false;
    
    public string Name { get; set; } = "";
    public string Tag { get; set; } = "";
    
    public bool IsVisible { get; set; } = true;
    public bool IsEnabled { get; set; } = true;
    public bool CanReceiveFocus { get; set; } = false;
    public bool HasFocus { get; set; } = false;
    public float BackgroundTransparency { get; set; } = 0f;
    
    // Selection navigation for console/gamepad support
    public IUIComponent NextSelection { get; set; }
    public IUIComponent PreviousSelection { get; set; }
    
    public virtual int RenderOrder => 0;
    public LayoutInfo Layout { get; set; } = new LayoutInfo();
    
    public virtual Rectangle Bounds => layoutDirty ? RecalculateBounds() : cachedBounds;
    public virtual Rectangle ContentBounds 
    { 
        get 
        {
            var bounds = Bounds;
            var padding = Layout.Padding;
            return new Rectangle(
                bounds.X + (int)padding.Left,
                bounds.Y + (int)padding.Top,
                bounds.Width - (int)(padding.Left + padding.Right),
                bounds.Height - (int)(padding.Top + padding.Bottom)
            );
        }
    }
    
    public IUIComponent Parent 
    { 
        get => parent;
        set
        {
            if (parent == value) return;
            parent?.RemoveChild(this);
            parent = value;
            if (parent != null && !parent.Children.Contains(this))
                parent.AddChild(this);
            InvalidateLayout();
        }
    }
    
    public IReadOnlyList<IUIComponent> Children => children.AsReadOnly();
    
    public event Action<IUIComponent> OnClick;
    public event Action<IUIComponent> OnPressed;
    public event Action<IUIComponent> OnReleased;
    public event Action<IUIComponent> OnMouseEnter;
    public event Action<IUIComponent> OnMouseExit;
    public event Action<IUIComponent> OnFocusGained;
    public event Action<IUIComponent> OnFocusLost;
    public event Action<IUIComponent, Keys> OnKeyPressed;
    
    // Hierarchy management
    public virtual void AddChild(IUIComponent child)
    {
        if (child == null || children.Contains(child) || child == this) return;
        
        child.Parent = this;
        children.Add(child);
        children.Sort((a, b) => a.RenderOrder.CompareTo(b.RenderOrder));
        InvalidateLayout();
    }
    
    public virtual void RemoveChild(IUIComponent child)
    {
        if (children.Remove(child))
        {
            child.Parent = null;
            InvalidateLayout();
        }
    }
    
    public virtual void ClearChildren()
    {
        foreach (var child in children)
            child.Parent = null;
        children.Clear();
        InvalidateLayout();
    }
    
    public T FindChild<T>(string name = null) where T : class, IUIComponent
    {
        foreach (var child in children)
        {
            if (child is T match && (name == null || child.Name == name))
                return match;
            
            var found = child.FindChild<T>(name);
            if (found != null) return found;
        }
        return null;
    }
    
    // Transform methods
    public virtual Vector2 LocalToWorld(Vector2 localPosition)
    {
        Vector2 worldPos = GetPositionInParent() + localPosition;
        return Parent?.LocalToWorld(worldPos) ?? worldPos;
    }
    
    public virtual Vector2 WorldToLocal(Vector2 worldPosition)
    {
        Vector2 localPos = Parent?.WorldToLocal(worldPosition) ?? worldPosition;
        return localPos - GetPositionInParent();
    }
    
    public virtual Rectangle GetWorldBounds()
    {
        Vector2 worldPos = LocalToWorld(Vector2.Zero);
        var bounds = Bounds;
        return new Rectangle((int)worldPos.X, (int)worldPos.Y, bounds.Width, bounds.Height);
    }
    
    public virtual bool ContainsPoint(Vector2 worldPoint)
    {
        return GetWorldBounds().Contains((int)worldPoint.X, (int)worldPoint.Y);
    }
    
    // Layout methods
    public virtual void InvalidateLayout()
    {
        layoutDirty = true;
        foreach (var child in children)
            child.InvalidateLayout();
    }
    
    public virtual void UpdateLayout()
    {
        if (layoutDirty)
        {
            RecalculateBounds();
            layoutDirty = false;
        }
        
        foreach (var child in children)
            child.UpdateLayout();
    }
    
    // Focus management methods
    public virtual void FocusGained()
    {
        if (!HasFocus)
        {
            HasFocus = true;
            OnFocusGained?.Invoke(this);
        }
    }
    
    public virtual void FocusLost()
    {
        if (HasFocus)
        {
            HasFocus = false;
            OnFocusLost?.Invoke(this);
        }
    }
    
    public virtual void Update(InterfaceManager ui)
    {
        if (!IsVisible) return;
        
        UpdateLayout();
        UpdateSelf(ui);
        
        bool mouseOver = IsEnabled && ContainsPoint(ui.MousePosition);
        
        if (mouseOver && !isHovered)
        {
            isHovered = true;
            OnMouseEnter?.Invoke(this);
        }
        else if (!mouseOver && isHovered)
        {
            isHovered = false;
            OnMouseExit?.Invoke(this);
        }
        
        if (HasFocus)
        {
            // Handle Tab navigation with explicit NextSelection/PreviousSelection
            if (ui.IsKeyPressed(Keys.Tab))
            {
                var nextComponent = ui.IsKeyDown(Keys.LeftShift) ? PreviousSelection : NextSelection;
                if (nextComponent != null && nextComponent.CanReceiveFocus && nextComponent.IsVisible && nextComponent.IsEnabled)
                {
                    ui.SetFocus(nextComponent);
                }
                else
                {
                    // Fallback to automatic sibling navigation
                    var fallbackComponent = FindNextFocusableComponent(!ui.IsKeyDown(Keys.LeftShift));
                    if (fallbackComponent != null)
                    {
                        ui.SetFocus(fallbackComponent);
                    }
                }
            }
            
            // Handle Enter/Space as click for console support
            if (ui.IsKeyPressed(Keys.Enter) || ui.IsKeyPressed(Keys.Space))
            {
                OnClick?.Invoke(this);
            }
            
            var pressedKeys = GetPressedKeys(ui);
            foreach (var key in pressedKeys)
            {
                OnKeyPressed?.Invoke(this, key);
            }
        }
        
        if (mouseOver && ui.IsLeftMousePressed())
        {
            isPressed = true;
            OnPressed?.Invoke(this);
        }
        
        if (isPressed && ui.IsLeftMouseReleased())
        {
            OnReleased?.Invoke(this);
            if (mouseOver)
                OnClick?.Invoke(this);
            isPressed = false;
        }
        
        foreach (var child in children.Where(c => c.IsVisible).OrderByDescending(c => c.RenderOrder))
        {
            child.Update(ui);
        }
    }
    
    private IUIComponent FindNextFocusableComponent(bool forward)
    {
        if (Parent == null) return null;
        
        var siblings = Parent.Children.Where(c => c.CanReceiveFocus && c.IsVisible && c.IsEnabled).ToList();
        var currentIndex = siblings.IndexOf(this);
        
        if (currentIndex == -1) return null;
        
        var nextIndex = forward ? currentIndex + 1 : currentIndex - 1;
        if (nextIndex >= siblings.Count) nextIndex = 0;
        if (nextIndex < 0) nextIndex = siblings.Count - 1;
        
        return nextIndex < siblings.Count ? siblings[nextIndex] : null;
    }
    
    private Keys[] GetPressedKeys(InterfaceManager ui)
    {
        var pressedKeys = new List<Keys>();
        foreach (Keys key in Enum.GetValues(typeof(Keys)))
        {
            if (ui.IsKeyPressed(key))
                pressedKeys.Add(key);
        }
        return pressedKeys.ToArray();
    }
    
    public virtual void Render(InterfaceManager ui)
    {
        if (!IsVisible) return;
        
        RenderSelf(ui);
        
        foreach (var child in children.Where(c => c.IsVisible).OrderBy(c => c.RenderOrder))
        {
            child.Render(ui);
        }
    }
    
    // Layout calculation methods
    private Rectangle RecalculateBounds()
    {
        Vector2 position = CalculatePosition();
        Vector2 size = CalculateSize();
        
        cachedBounds = new Rectangle((int)position.X, (int)position.Y, (int)size.X, (int)size.Y);
        return cachedBounds;
    }
    
    private Vector2 CalculatePosition()
    {
        Vector2 position = Vector2.Zero;
        
        if (Parent != null)
        {
            var parentBounds = Parent.ContentBounds;
            
            if (Layout.RelativePosition.HasValue)
            {
                position = new Vector2(
                    parentBounds.X + parentBounds.Width * Layout.RelativePosition.Value.X,
                    parentBounds.Y + parentBounds.Height * Layout.RelativePosition.Value.Y
                );
            }
            else
            {
                position = new Vector2(parentBounds.X, parentBounds.Y);
            }
            
            position += GetAnchorOffset(parentBounds, Layout.Anchor);
        }
        
        if (Layout.AbsolutePosition.HasValue)
            position = Layout.AbsolutePosition.Value;
        if (Layout.Offset.HasValue)
            position += Layout.Offset.Value;
        
        position += new Vector2(Layout.Margin.Left, Layout.Margin.Top);
        
        var size = CalculateSize();
        position -= new Vector2(size.X * Layout.Pivot.X, size.Y * Layout.Pivot.Y);
        
        return position;
    }
    
    private Vector2 CalculateSize()
    {
        Vector2 size = new Vector2(100, 50);
        
        if (Layout.AbsoluteSize.HasValue)
        {
            size = Layout.AbsoluteSize.Value;
        }
        else if (Layout.RelativeSize.HasValue && Parent != null)
        {
            var parentBounds = Parent.ContentBounds;
            size = new Vector2(
                parentBounds.Width * Layout.RelativeSize.Value.X,
                parentBounds.Height * Layout.RelativeSize.Value.Y
            );
        }
        else if (Layout.FillParent && Parent != null)
        {
            var parentBounds = Parent.ContentBounds;
            size = new Vector2(
                parentBounds.Width - Layout.Margin.Left - Layout.Margin.Right,
                parentBounds.Height - Layout.Margin.Top - Layout.Margin.Bottom
            );
        }
        
        if (Layout.MinSize.HasValue)
        {
            size.X = MathHelper.Max(size.X, Layout.MinSize.Value.X);
            size.Y = MathHelper.Max(size.Y, Layout.MinSize.Value.Y);
        }
        if (Layout.MaxSize.HasValue)
        {
            size.X = MathHelper.Min(size.X, Layout.MaxSize.Value.X);
            size.Y = MathHelper.Min(size.Y, Layout.MaxSize.Value.Y);
        }
        
        return size;
    }
    
    private Vector2 GetPositionInParent()
    {
        var bounds = Bounds;
        return new Vector2(bounds.X, bounds.Y);
    }
    
    private Vector2 GetAnchorOffset(Rectangle parentBounds, Anchor anchor)
    {
        return anchor switch
        {
            Anchor.TopLeft => Vector2.Zero,
            Anchor.TopCenter => new Vector2(parentBounds.Width * 0.5f, 0),
            Anchor.TopRight => new Vector2(parentBounds.Width, 0),
            Anchor.MiddleLeft => new Vector2(0, parentBounds.Height * 0.5f),
            Anchor.MiddleCenter => new Vector2(parentBounds.Width * 0.5f, parentBounds.Height * 0.5f),
            Anchor.MiddleRight => new Vector2(parentBounds.Width, parentBounds.Height * 0.5f),
            Anchor.BottomLeft => new Vector2(0, parentBounds.Height),
            Anchor.BottomCenter => new Vector2(parentBounds.Width * 0.5f, parentBounds.Height),
            Anchor.BottomRight => new Vector2(parentBounds.Width, parentBounds.Height),
            _ => Vector2.Zero
        };
    }
    
    protected abstract void UpdateSelf(InterfaceManager ui);
    protected abstract void RenderSelf(InterfaceManager ui);
}

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