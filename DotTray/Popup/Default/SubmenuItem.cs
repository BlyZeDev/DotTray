namespace DotTray.Popup.Default;

using DotTray.Popup.Default.Context;
using DotTray.Primitives;
using System;

/// <summary>
/// Represents a basic popup menu item that includes a submenu
/// </summary>
public class SubmenuItem : MenuItem, ISubmenu
{
    private bool isHovering;

    private const int ArrowHeightRatio = 3;
    private const int ArrowGap = 8;
    private const int ArrowRightPadding = 10;

    /// <inheritdoc/>
    public MenuItemCollection Items { get; } = [];

    /// <inheritdoc/>
    protected internal override Size Measure(MeasuringContext context)
    {
        var baseSize = base.Measure(context);
        if (Items.IsEmpty) return baseSize;

        var arrowHeight = Math.Max(6, baseSize.Height / ArrowHeightRatio);
        var arrowWidth = Math.Max(4, arrowHeight * 2 / 3);

        return new Size(baseSize.Width + ArrowGap + arrowWidth + ArrowRightPadding, baseSize.Height);
    }

    /// <inheritdoc/>
    protected internal override void Draw(DrawingContext context)
    {
        if (Items.IsEmpty)
        {
            base.Draw(context);
            return;
        }

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

        var background = IsDisabled ? BackgroundDisabled : (isHovering ? BackgroundHover : Background);
        var foreground = IsDisabled ? ForegroundDisabled : (isHovering ? ForegroundHover : Foreground);

        context.Fill(background);
        context.WriteRect(textBounds, Text, FontInfo, foreground);
        context.FillPolygon(foreground, arrow);
    }

    /// <inheritdoc/>
    internal protected override void OnInteraction(ItemInteractedEventArgs args)
    {
        switch (args.Type)
        {
            case ItemInteractionType.MouseEnter: isHovering = true; Update(); break;
            case ItemInteractionType.MouseLeave: isHovering = false; Update(); break;
        }

        base.OnInteraction(args);
    }

    /// <inheritdoc cref="ISubmenu.ShouldOpen(ItemInteractedEventArgs)"/>
    protected virtual bool ShouldOpen(ItemInteractedEventArgs args)
    {
        return !IsDisabled && !Items.IsEmpty && args.Type is ItemInteractionType.MouseEnter;
    }

    bool ISubmenu.ShouldOpen(ItemInteractedEventArgs args) => ShouldOpen(args);
}