namespace DotTray.Direct.Items;

using DotTray.Direct.Coloring;
using DotTray.Direct.Context;
using DotTray.Direct.Primitives;
using DotTray.Direct.Typography;
using System;

/// <summary>
/// Represents a popup menu item
/// </summary>
public class MenuItem : MenuItemBase
{
    private const int ArrowHeightRatio = 3;
    private const int ArrowGap = 8;
    private const int ArrowRightPadding = 10;

    private int leadingWidth;
    private int trailingWidth;

    /// <summary>
    /// <see langword="true"/> if this instance is hovered over or focused by the keyboard navigation, otherwise <see langword="false"/>
    /// </summary>
    protected bool isHovering;

    /// <summary>
    /// The background brush matching the current state (disabled, hovered or normal)
    /// </summary>
    protected IDirectBrush CurrentBackground => IsDisabled ? BackgroundDisabled : (isHovering ? BackgroundHover : Background);

    /// <summary>
    /// The foreground brush matching the current state (disabled, hovered or normal)
    /// </summary>
    protected IDirectBrush CurrentForeground => IsDisabled ? ForegroundDisabled : (isHovering ? ForegroundHover : Foreground);

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <remarks>
    /// This is the same value as <see cref="IsDisabled"/> and therefore can be ignored
    /// </remarks>
    internal protected sealed override bool IgnoreInteraction => IsDisabled;
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <remarks>
    /// This returns the same reference as <see cref="Items"/> and therefore can be ignored
    /// </remarks>
    internal protected sealed override MenuItemCollection SubmenuItems => base.SubmenuItems;

    /// <summary>
    /// The background color
    /// </summary>
    public IDirectBrush Background
    {
        get;
        set
        {
            if (field.Equals(value)) return;

            field = value;
            Update();
        }
    } = DirectDefaultStyles.ItemBackground;

    /// <summary>
    /// The foreground color
    /// </summary>
    public IDirectBrush Foreground
    {
        get;
        set
        {
            if (field.Equals(value)) return;

            field = value;
            Update();
        }
    } = DirectDefaultStyles.ItemForeground;

    /// <summary>
    /// The background hover color
    /// </summary>
    public IDirectBrush BackgroundHover
    {
        get;
        set
        {
            if (field.Equals(value)) return;

            field = value;
            Update();
        }
    } = DirectDefaultStyles.ItemBackgroundHover;

    /// <summary>
    /// The foreground hover color
    /// </summary>
    public IDirectBrush ForegroundHover
    {
        get;
        set
        {
            if (field.Equals(value)) return;

            field = value;
            Update();
        }
    } = DirectDefaultStyles.ItemForegroundHover;

    /// <summary>
    /// The background disabled color
    /// </summary>
    public IDirectBrush BackgroundDisabled
    {
        get;
        set
        {
            if (field.Equals(value)) return;

            field = value;
            Update();
        }
    } = DirectDefaultStyles.ItemBackgroundDisabled;

    /// <summary>
    /// The foreground disabled color
    /// </summary>
    public IDirectBrush ForegroundDisabled
    {
        get;
        set
        {
            if (field.Equals(value)) return;

            field = value;
            Update();
        }
    } = DirectDefaultStyles.ItemForegroundDisabled;

    /// <summary>
    /// The displayed text
    /// </summary>
    public string Text
    {
        get;
        set
        {
            if (string.Equals(field, value, StringComparison.Ordinal)) return;

            field = value;
            Update();
        }
    } = "";

    /// <summary>
    /// The text info used to display the text
    /// </summary>
    public TextInfo TextInfo
    {
        get;
        set
        {
            if (field.Equals(value)) return;

            field = value;
            Update();
        }
    } = TextInfo.Default;

    /// <summary>
    /// <see langword="true"/> to disable this instance, otherwise <see langword="false"/>
    /// </summary>
    public bool IsDisabled
    {
        get;
        set
        {
            if (field.Equals(value)) return;

            field = value;
            Update();
        }
    } = false;

    /// <summary>
    /// Items that should be shown in a submenu popup
    /// </summary>
    /// <remarks>
    /// If <see cref="MenuItemCollection.IsEmpty"/>, no submenu popup will appear
    /// </remarks>
    public MenuItemCollection Items => SubmenuItems;

    /// <summary>
    /// Default configuration for <see cref="MenuItem"/>
    /// </summary>
    public MenuItem() { }

    /// <inheritdoc/>
    internal protected override void Initialize() => isHovering = false;

    /// <summary>
    /// Measures the content area, which is the <see cref="Text"/> by default
    /// </summary>
    /// <param name="context">Context for measuring this instance</param>
    /// <returns>The size the content needs, in DIPs</returns>
    protected virtual Dim MeasureContent(MeasuringContext context)
    {
        var text = context.MeasureText(Text, TextInfo);
        return new Dim((int)MathF.Ceiling(text.Width * 1.05f), (int)MathF.Ceiling(text.Height * 1.05f));
    }

    /// <summary>
    /// Measures the leading area, which is the space left of the content
    /// </summary>
    /// <remarks>
    /// Empty by default. The returned width is reserved, the height only contributes to the item's height
    /// </remarks>
    /// <param name="context">Context for measuring this instance</param>
    /// <param name="contentSize">The size measured by <see cref="MeasureContent(MeasuringContext)"/></param>
    /// <returns>The size the leading area needs, in DIPs</returns>
    protected virtual Dim MeasureLeading(MeasuringContext context, Dim contentSize) => new(0, 0);

    /// <summary>
    /// Measures the trailing area, which is the space right of the content
    /// </summary>
    /// <remarks>
    /// By default this reserves space for the submenu arrow if there are <see cref="Items"/>, otherwise it is empty.<br/>
    /// Override and call the base implementation to keep the arrow, or don't to replace it
    /// </remarks>
    /// <param name="context">Context for measuring this instance</param>
    /// <param name="contentSize">The size measured by <see cref="MeasureContent(MeasuringContext)"/></param>
    /// <returns>The size the trailing area needs, in DIPs</returns>
    protected virtual Dim MeasureTrailing(MeasuringContext context, Dim contentSize)
    {
        if (SubmenuItems.IsEmpty) return new Dim(0, 0);

        GetArrowSize(contentSize.Height, out var arrowWidth, out _);
        return new Dim(ArrowGap + arrowWidth + ArrowRightPadding, 0);
    }

    /// <summary>
    /// Draws the background of the whole item
    /// </summary>
    /// <param name="context">Context for drawing this instance</param>
    /// <param name="bounds">The bounds of the whole item</param>
    protected virtual void DrawBackground(DrawingContext context, Rect bounds) => context.FillRect(bounds, CurrentBackground);

    /// <summary>
    /// Draws the leading area
    /// </summary>
    /// <remarks>
    /// Only called if <see cref="MeasureLeading(MeasuringContext, Dim)"/> reserved a width greater than <c>0</c>
    /// </remarks>
    /// <param name="context">Context for drawing this instance</param>
    /// <param name="bounds">The bounds of the leading area, spanning the full item height</param>
    protected virtual void DrawLeading(DrawingContext context, Rect bounds) { }

    /// <summary>
    /// Draws the content area, which is the <see cref="Text"/> by default
    /// </summary>
    /// <param name="context">Context for drawing this instance</param>
    /// <param name="bounds">The bounds of the content area, spanning the full item height</param>
    protected virtual void DrawContent(DrawingContext context, Rect bounds) => context.WriteRect(bounds, Text, TextInfo, CurrentForeground);

    /// <summary>
    /// Draws the trailing area
    /// </summary>
    /// <remarks>
    /// Only called if <see cref="MeasureTrailing(MeasuringContext, Dim)"/> reserved a width greater than <c>0</c>.<br/>
    /// By default this draws the submenu arrow
    /// </remarks>
    /// <param name="context">Context for drawing this instance</param>
    /// <param name="bounds">The bounds of the trailing area, spanning the full item height</param>
    protected virtual void DrawTrailing(DrawingContext context, Rect bounds)
    {
        if (SubmenuItems.IsEmpty) return;

        GetArrowSize(bounds.Height, out var arrowWidth, out var arrowHeight);

        var arrowBounds = new Rect(
            bounds.Right - ArrowRightPadding - arrowWidth,
            bounds.Top + (bounds.Height - arrowHeight) / 2,
            arrowWidth,
            arrowHeight);

        context.DrawChevron(arrowBounds, CurrentForeground);
    }

    /// <inheritdoc/>
    internal protected override Dim Measure(MeasuringContext context)
    {
        var content = MeasureContent(context);
        var leading = MeasureLeading(context, content);
        var trailing = MeasureTrailing(context, content);

        leadingWidth = leading.Width;
        trailingWidth = trailing.Width;

        return new Dim(
            leading.Width + content.Width + trailing.Width,
            Math.Max(content.Height, Math.Max(leading.Height, trailing.Height)));
    }

    /// <inheritdoc/>
    internal protected override void Draw(DrawingContext context)
    {
        var bounds = context.ItemBounds;

        var leadingBounds = new Rect(bounds.X, bounds.Y, leadingWidth, bounds.Height);
        var trailingBounds = new Rect(bounds.Right - trailingWidth, bounds.Y, trailingWidth, bounds.Height);
        var contentBounds = new Rect(bounds.X + leadingWidth, bounds.Y, bounds.Width - leadingWidth - trailingWidth, bounds.Height);

        DrawBackground(context, bounds);

        if (leadingWidth > 0) DrawLeading(context, leadingBounds);
        DrawContent(context, contentBounds);
        if (trailingWidth > 0) DrawTrailing(context, trailingBounds);
    }

    /// <inheritdoc/>
    internal protected override void OnInteraction(ItemInteractedEventArgs args)
    {
        switch (args.Type)
        {
            case ItemInteractionType.MouseEnter or ItemInteractionType.KeyboardFocus:
                isHovering = true;
                Update();
                break;

            case ItemInteractionType.MouseLeave or ItemInteractionType.KeyboardBlur:
                isHovering = false;
                Update();
                break;
        }

        base.OnInteraction(args);
    }

    private static void GetArrowSize(int itemHeight, out int width, out int height)
    {
        height = Math.Max(6, itemHeight / ArrowHeightRatio);
        width = Math.Max(4, height * 2 / 3);
    }
}