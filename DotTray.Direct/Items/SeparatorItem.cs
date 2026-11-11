namespace DotTray.Direct.Items;

using DotTray.Direct.Coloring;
using DotTray.Direct.Context;
using DotTray.Direct.Primitives;
using System;

/// <summary>
/// Represents a purely visual popup menu item that draws a horizontal separator line
/// </summary>
public class SeparatorItem : MenuItemBase
{
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <remarks>
    /// A separator is always purely visual, so this is always <see langword="true"/>
    /// </remarks>
    internal protected override bool IgnoreInteraction => true;

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
    /// The color of the separator line
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
    } = DirectDefaultStyles.ItemForegroundDisabled;

    /// <summary>
    /// The thickness of the separator line, in DIPs
    /// </summary>
    public int Thickness
    {
        get;
        set
        {
            value = Math.Max(1, value);
            if (field == value) return;

            field = value;
            Update();
        }
    } = 1;

    /// <summary>
    /// The empty space above and below the separator line, in DIPs
    /// </summary>
    public int VerticalPadding
    {
        get;
        set
        {
            value = Math.Max(0, value);
            if (field == value) return;

            field = value;
            Update();
        }
    } = 0;

    /// <summary>
    /// The empty space left and right of the separator line, in DIPs
    /// </summary>
    public int HorizontalPadding
    {
        get;
        set
        {
            value = Math.Max(0, value);
            if (field == value) return;

            field = value;
            Update();
        }
    } = 0;

    /// <summary>
    /// <see langword="true"/> to draw the separator line with rounded ends, otherwise <see langword="false"/>
    /// </summary>
    public bool IsRounded
    {
        get;
        set
        {
            if (field.Equals(value)) return;

            field = value;
            Update();
        }
    } = true;

    /// <summary>
    /// Default configuration for <see cref="SeparatorItem"/>
    /// </summary>
    public SeparatorItem() { }

    /// <inheritdoc/>
    internal protected override Dim Measure(MeasuringContext context) => new Dim(0, Thickness + VerticalPadding * 2);

    /// <inheritdoc/>
    internal protected override void Draw(DrawingContext context)
    {
        var bounds = context.ItemBounds;

        context.FillRect(bounds, Background);

        var px = Math.Max(1f, MathF.Round(Thickness * context.Scale));
        var top = bounds.Y + MathF.Floor((bounds.Height * context.Scale - px) / 2f) / context.Scale;
        var line = new RectF(bounds.X + HorizontalPadding, top, Math.Max(0, bounds.Width - HorizontalPadding * 2), px / context.Scale);

        if (IsRounded) context.FillRoundedRect(line, Thickness / 2f, Foreground);
        else context.FillRect(line, Foreground);
    }
}