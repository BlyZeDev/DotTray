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
    private const float ArrowWidthRatio = 2f / 3f;
    private const float ArrowGapRatio = 1f;
    private const float ArrowRightPaddingRatio = 1f;

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

        return new Dim(content.Width + GetArrowReservedWidth(context.GetFontMetrics(TextInfo)), content.Height);
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

        var metrics = context.GetFontMetrics(TextInfo);
        GetArrowMetrics(metrics, out var arrowWidth, out var arrowHeight, out _, out var padding);

        var contentWidth = bounds.Width - GetArrowReservedWidth(metrics);
        DrawContent(context, new Rect(bounds.X, bounds.Y, contentWidth, bounds.Height));

        context.DrawChevron(new RectF(
            bounds.Right - padding - arrowWidth,
            bounds.Top + metrics.CapCenterIn(bounds.Height) - arrowHeight / 2f,
            arrowWidth,
            arrowHeight), CurrentForeground, thickness: metrics.StrokeThicknessOrEstimate);
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

    private static int GetArrowReservedWidth(FontMetrics metrics)
    {
        GetArrowMetrics(metrics, out var width, out _, out var gap, out var padding);
        return (int)MathF.Ceiling(gap + width + padding);
    }

    private static void GetArrowMetrics(FontMetrics metrics, out float width, out float height, out float gap, out float padding)
    {
        var cap = metrics.CapHeightOrEstimate;

        height = cap;
        width = height * ArrowWidthRatio;
        gap = cap * ArrowGapRatio;
        padding = cap * ArrowRightPaddingRatio;
    }
}