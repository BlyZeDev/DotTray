namespace DotTray.Popup.Default;

using DotTray.Popup.Default.Context;
using DotTray.Primitives;
using System;

/// <summary>
/// Represents a basic popup menu item that includes a submenu
/// </summary>
public class SubmenuItem : MenuItem, ISubmenu
{
    private const int ArrowHeightRatio = 3;
    private const int ArrowGap = 8;
    private const int ArrowRightPadding = 10;

    /// <inheritdoc/>
    public MenuItemCollection Items { get; } = [];

    /// <inheritdoc/>
    protected internal override Size Measure(MeasuringContext context)
    {
        var baseSize = base.Measure(context);

        var arrowHeight = Math.Max(6, baseSize.Height / ArrowHeightRatio);
        var arrowWidth = Math.Max(4, arrowHeight * 2 / 3);

        return new Size(baseSize.Width + ArrowGap + arrowWidth + ArrowRightPadding, baseSize.Height);
    }

    /// <inheritdoc/>
    protected internal override void Draw(DrawingContext context)
    {
        var bounds = context.ItemBounds;

        var arrowHeight = Math.Max(6, bounds.Height / ArrowHeightRatio);
        var arrowWidth = Math.Max(4, arrowHeight * 2 / 3);

        var arrowX = bounds.Right - ArrowRightPadding - arrowWidth;
        var arrowY = bounds.Top + (bounds.Height - arrowHeight) / 2;

        var thickness = Math.Max(1, arrowHeight / 5);

        var centerY = arrowY + arrowHeight / 2;

        ReadOnlySpan<Point> arrow =
        [
            new Point(arrowX, arrowY),
            new Point(arrowX + thickness, arrowY),
            new Point(arrowX + arrowWidth, centerY),
            new Point(arrowX + thickness, arrowY + arrowHeight),
            new Point(arrowX, arrowY + arrowHeight),
            new Point(arrowX + arrowWidth - thickness, centerY)
        ];

        var textBounds = new Rectangle(bounds.X, bounds.Y, bounds.Width - arrowWidth - ArrowGap - ArrowRightPadding, bounds.Height);

        context.Fill(Background);
        context.WriteRect(textBounds, Text, FontInfo, Foreground);
        context.FillPolygon(Foreground, arrow);
    }

    /// <inheritdoc cref="ISubmenu.ShouldOpen(ItemInteractedEventArgs)"/>
    protected virtual bool ShouldOpen(ItemInteractedEventArgs args) => !Items.IsEmpty && args.Type is ItemInteractionType.MouseEnter;

    bool ISubmenu.ShouldOpen(ItemInteractedEventArgs args) => ShouldOpen(args);
}