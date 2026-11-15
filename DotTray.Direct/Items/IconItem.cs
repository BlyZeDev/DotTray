namespace DotTray.Direct.Items;

using DotTray.Direct.Context;
using DotTray.Direct.Imaging;
using DotTray.Direct.Primitives;
using DotTray.Direct.Typography;

/// <summary>
/// Represents a popup menu item that shows an icon in front of its text
/// </summary>
public class IconItem : MenuItem
{
    private const float PaddingRatio = 0.3f;
    private const float VerticalPaddingRatio = 0.15f;
    private const float GapRatio = 0.6f;
    private const float DisabledOpacity = 0.4f;

    /// <summary>
    /// The icon to display, or <see langword="null"/> to only reserve the space of the icon
    /// </summary>
    /// <remarks>
    /// The image is scaled to fit the icon box while keeping its aspect ratio. The same <see cref="DirectImage"/> can be shared by many items
    /// </remarks>
    public virtual DirectImage? Icon
    {
        get;
        set
        {
            if (ReferenceEquals(field, value)) return;

            field = value;
            Update();
        }
    } = null;

    /// <summary>
    /// The width and height of the square icon box, in DIPs, or <see langword="null"/> to match the line height of the text
    /// </summary>
    public virtual int? IconSize
    {
        get;
        set
        {
            if (value is <= 0) value = null;
            if (field == value) return;

            field = value;
            Update();
        }
    } = null;

    /// <summary>
    /// The options the icon is drawn with
    /// </summary>
    /// <remarks>
    /// While the item is disabled, the opacity is additionally reduced
    /// </remarks>
    public virtual ImageOptions IconOptions
    {
        get;
        set
        {
            if (Equals(field, value)) return;

            field = value;
            Update();
        }
    } = ImageOptions.Default;

    /// <summary>
    /// Default configuration for <see cref="IconItem"/>
    /// </summary>
    public IconItem() { }

    /// <summary>
    /// Draws the icon, which is the <see cref="Icon"/> fitted into <paramref name="box"/> by default
    /// </summary>
    /// <remarks>
    /// The box is square and vertically centered in the item. This is not called if <see cref="Icon"/> is <see langword="null"/>
    /// </remarks>
    /// <param name="context">Context for drawing this instance</param>
    /// <param name="icon">The icon to draw</param>
    /// <param name="box">The bounds of the icon box</param>
    protected virtual void DrawIcon(DrawingContext context, DirectImage icon, RectF box)
    {
        var dimensions = icon.Dimensions;
        if (dimensions.Width <= 0 || dimensions.Height <= 0) return;

        var ratio = MathF.Min(box.Width / dimensions.Width, box.Height / dimensions.Height);
        var width = dimensions.Width * ratio;
        var height = dimensions.Height * ratio;

        var options = IconOptions;
        if (IsDisabled) options = options with { Opacity = options.Opacity * DisabledOpacity };

        context.DrawImage(icon, new RectF(box.X + (box.Width - width) / 2f, box.Y + (box.Height - height) / 2f, width, height), options);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Adds the space of the icon in front of the text, which is reserved even without an <see cref="Icon"/> so the texts of all items line up
    /// </remarks>
    protected override Dim MeasureContent(MeasuringContext context)
    {
        var content = base.MeasureContent(context);
        var metrics = context.GetFontMetrics(TextInfo);

        var iconHeight = GetIconSize(metrics) + (IconSize is null ? 0 : (int)MathF.Ceiling(GetVerticalPadding(metrics) * 2f));

        return new Dim(GetIconOffset(metrics) + content.Width, Math.Max(content.Height, iconHeight));
    }

    /// <inheritdoc/>
    protected override void DrawContent(DrawingContext context, Rect bounds)
    {
        var metrics = context.GetFontMetrics(TextInfo);
        var size = GetIconSize(metrics);

        if (Icon is not null) DrawIcon(context, Icon, new RectF(bounds.X + GetPadding(metrics), bounds.Y + (bounds.Height - size) / 2f, size, size));

        var offset = GetIconOffset(metrics);
        base.DrawContent(context, new Rect(bounds.X + offset, bounds.Y, bounds.Width - offset, bounds.Height));
    }

    private int GetIconSize(FontMetrics metrics) => IconSize ?? Math.Max(1, (int)MathF.Floor(metrics.LineHeight - GetVerticalPadding(metrics) * 2f));
    private int GetIconOffset(FontMetrics metrics) => (int)MathF.Ceiling(GetPadding(metrics) + GetIconSize(metrics) + metrics.CapHeightOrEstimate * GapRatio);

    private static float GetPadding(FontMetrics metrics) => metrics.CapHeightOrEstimate * PaddingRatio;
    private static float GetVerticalPadding(FontMetrics metrics) => metrics.CapHeightOrEstimate * VerticalPaddingRatio;
}