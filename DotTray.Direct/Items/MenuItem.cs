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

    /// <summary>
    /// <see langword="true"/> if this instance is hovered over or focused by the keyboard navigation, otherwise <see langword="false"/>
    /// </summary>
    protected bool isHovering;

    /// <summary>
    /// The background brush matching the current state (disabled, hovered or normal)
    /// </summary>
    protected virtual IDirectBrush CurrentBackground => IsDisabled ? BackgroundDisabled : (isHovering ? BackgroundHover : Background);

    /// <summary>
    /// The foreground brush matching the current state (disabled, hovered or normal)
    /// </summary>
    protected virtual IDirectBrush CurrentForeground => IsDisabled ? ForegroundDisabled : (isHovering ? ForegroundHover : Foreground);

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <remarks>
    /// This is the same value as <see cref="IsDisabled"/>
    /// </remarks>
    internal protected sealed override bool IgnoreInteraction => IsDisabled;
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <remarks>
    /// This value is superseded by <see cref="Items"/>
    /// </remarks>
    internal protected sealed override MenuItemCollection SubmenuItems => base.SubmenuItems;

    /// <summary>
    /// The background color
    /// </summary>
    public virtual IDirectBrush Background
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
    public virtual IDirectBrush Foreground
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
    public virtual IDirectBrush BackgroundHover
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
    public virtual IDirectBrush ForegroundHover
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
    public virtual IDirectBrush BackgroundDisabled
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
    public virtual IDirectBrush ForegroundDisabled
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
    public virtual string Text
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
    public virtual TextInfo TextInfo
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
    public virtual bool IsDisabled
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
    /// Draws the background of the whole item
    /// </summary>
    /// <param name="context">Context for drawing this instance</param>
    /// <param name="bounds">The bounds of the whole item</param>
    protected virtual void DrawBackground(DrawingContext context, Rect bounds) => context.FillRect(bounds, CurrentBackground);

    /// <summary>
    /// Draws the content area, which is the <see cref="Text"/> by default
    /// </summary>
    /// <param name="context">Context for drawing this instance</param>
    /// <param name="bounds">The bounds of the content area, spanning the full item height</param>
    protected virtual void DrawContent(DrawingContext context, Rect bounds) => context.WriteRect(bounds, Text, TextInfo, CurrentForeground);

    /// <inheritdoc/>
    /// <remarks>
    /// The size is <see cref="MeasureContent(MeasuringContext)"/> plus the space of the submenu arrow, if there is one
    /// </remarks>
    internal protected override Dim Measure(MeasuringContext context)
    {
        var content = MeasureContent(context);
        if (SubmenuItems.IsEmpty) return content;

        GetArrowSize(content.Height, out var arrowWidth, out _);
        return new Dim(content.Width + ArrowGap + arrowWidth + ArrowRightPadding, content.Height);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Draws the background, then the content into the bounds left over by the submenu arrow, then the arrow
    /// </remarks>
    internal protected override void Draw(DrawingContext context)
    {
        var bounds = context.ItemBounds;

        DrawBackground(context, bounds);

        if (SubmenuItems.IsEmpty)
        {
            DrawContent(context, bounds);
            return;
        }

        GetArrowSize(bounds.Height, out var arrowWidth, out var arrowHeight);

        DrawContent(context, new Rect(bounds.X, bounds.Y, bounds.Width - ArrowGap - arrowWidth - ArrowRightPadding, bounds.Height));

        context.DrawChevron(new Rect(
            bounds.Right - ArrowRightPadding - arrowWidth,
            bounds.Top + (bounds.Height - arrowHeight) / 2,
            arrowWidth,
            arrowHeight), CurrentForeground);
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