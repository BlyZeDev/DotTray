namespace DotTray.Drawing.Items;

using DotTray.Drawing;
using DotTray.Drawing.Context;
using DotTray.Drawing.Primitives;
using DotTray.Primitives;
using System;
using System.Drawing;

/// <summary>
/// Represents a basic popup separator item
/// </summary>
public class SeparatorItem : MenuItemBase
{
    /// <inheritdoc/>
    internal protected sealed override bool IgnoreInteraction => true;

    /// <summary>
    /// The line color
    /// </summary>
    public Brush LineColor
    {
        get;
        set
        {
            if (field.Equals(value)) return;

            field = value;
            Update();
        }
    } = SystemBrushes.ControlDark;

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
    internal protected override Dim Measure(MeasuringContext context) => new Dim(Padding.Horizontal, LineHeight + Padding.Vertical);

    /// <inheritdoc/>
    internal protected override void Draw(DrawingContext context)
    {
        context.Graphics.FillRectangle(LineColor, context.ItemBounds with
        {
            X = context.ItemBounds.X + Padding.Left,
            Y = context.ItemBounds.Y + Padding.Top,
            Width = Math.Max(0, context.ItemBounds.Width - Padding.Horizontal),
            Height = LineHeight
        });
    }
}