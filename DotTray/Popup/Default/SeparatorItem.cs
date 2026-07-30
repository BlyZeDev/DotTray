namespace DotTray.Popup.Default;

using DotTray.Popup.Default.Coloring;
using DotTray.Popup.Default.Context;
using DotTray.Primitives;
using System;

/// <summary>
/// Represents a basic popup separator item
/// </summary>
public class SeparatorItem : MenuItemBase
{
    /// <summary>
    /// The line color
    /// </summary>
    public IColorable LineColor
    {
        get;
        set
        {
            if (field.Equals(value)) return;

            field = value;
            Update();
        }
    } = SolidColor.Black;

    /// <summary>
    /// The line height
    /// </summary>
    public int LineHeight
    {
        get;
        set
        {
            if (field.Equals(value)) return;

            field = value;
            Update();
        }
    } = 2;

    /// <summary>
    /// The padding around <see cref="LineHeight"/>
    /// </summary>
    public Padding Padding
    {
        get;
        set
        {
            if (field.Equals(value)) return;

            field = value;
            Update();
        }
    } = new Padding(8, 4);

    /// <inheritdoc/>
    internal protected override Size Measure(MeasuringContext context) => new Size(Padding.Horizontal, LineHeight + Padding.Vertical);

    /// <inheritdoc/>
    internal protected override void Draw(DrawingContext context)
    {
        context.FillRect(context.ItemBounds with
        {
            X = context.ItemBounds.X + Padding.Left,
            Y = context.ItemBounds.Y + Padding.Top,
            Width = Math.Max(0, context.ItemBounds.Width - Padding.Horizontal),
            Height = LineHeight
        }, LineColor);
    }

    /// <inheritdoc/>
    protected internal override void OnInteraction(ItemInteractedEventArgs args) { }
}