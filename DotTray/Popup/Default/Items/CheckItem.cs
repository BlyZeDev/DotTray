namespace DotTray.Popup.Default.Items;

using DotTray.Popup.Default.Context;
using DotTray.Primitives;
using System;

/// <summary>
/// Represents a checkable popup menu item
/// </summary>
public class CheckItem : MenuItem
{
    private const int CheckLeftPadding = 10;
    private const int CheckGap = 8;

    private Rectangle checkBounds;

    /// <summary>
    /// <see langword="true"/> if this instance is checked, otherwise <see langword="false"/>
    /// </summary>
    public bool IsChecked
    {
        get;
        set
        {
            if (field.Equals(value)) return;

            field = value;
            Update();
        }
    }

    /// <inheritdoc/>
    internal protected override Size Measure(MeasuringContext context)
    {
        var baseSize = base.Measure(context);
        var checkSize = Math.Max(10, baseSize.Height / 2);

        return baseSize with { Width = baseSize.Width + CheckLeftPadding + checkSize + CheckGap };
    }

    /// <inheritdoc/>
    internal protected override Rectangle Arrange(ArrangingContext context)
    {
        var itemBounds = context.ItemBounds;

        var checkSize = Math.Max(10, itemBounds.Height / 2);
        var checkAreaWidth = CheckLeftPadding + checkSize + CheckGap;

        checkBounds = new Rectangle(itemBounds.X + CheckLeftPadding, itemBounds.Y + (itemBounds.Height - checkSize) / 2, checkSize, checkSize);

        return itemBounds with { X = itemBounds.X + checkAreaWidth, Width = itemBounds.Width - checkAreaWidth };
    }

    /// <inheritdoc/>
    internal protected override void Draw(DrawingContext context)
    {
        base.Draw(context);

        var foreground = IsDisabled ? ForegroundDisabled : (isHovering ? ForegroundHover : Foreground);
        var background = IsDisabled ? BackgroundDisabled : (isHovering ? BackgroundHover : Background);

        var gutter = new Rectangle(checkBounds.X - CheckLeftPadding, context.ItemBounds.Y, checkBounds.Width + CheckLeftPadding + CheckGap, context.ItemBounds.Height);
        context.FillRect(gutter, background);

        if (!IsChecked) return;

        var (checkX, checkY, checkWidth, checkHeight) = (checkBounds.X, checkBounds.Y, checkBounds.Width, checkBounds.Height);

        var tVert = Math.Max(2, checkHeight / 4);
        tVert += tVert % 2;
        var tVertHalf = tVert / 2;

        var shortArm = Math.Max(tVertHalf + 2, checkWidth * 35 / 100);
        var longArm = Math.Max(tVertHalf + 5, checkWidth * 64 / 100);

        if (shortArm + longArm > checkWidth) longArm = checkWidth - shortArm;

        var left = checkX + (checkWidth - (shortArm + longArm)) / 2;
        var top = checkY + tVertHalf + (checkHeight - (longArm + tVertHalf)) / 2;

        var p0X = left;
        var p0Y = top + longArm - shortArm;

        var p1X = left + shortArm;
        var p1Y = top + longArm;

        var p2X = left + shortArm + longArm;

        ReadOnlySpan<Point> checkmark =
        [
            new Point(p0X, p0Y),
            new Point(p1X, p1Y),
            new Point(p2X, top),
            new Point(p2X - tVertHalf, top - tVertHalf),
            new Point(p1X, p1Y - tVert),
            new Point(p0X + tVertHalf, p0Y - tVertHalf)
        ];

        context.FillPolygon(foreground, checkmark);
    }

    /// <inheritdoc/>
    internal protected override void OnInteraction(ItemInteractedEventArgs args)
    {
        if (args.Type is ItemInteractionType.MouseLeftUp or ItemInteractionType.KeyboardActivate)
        {
            IsChecked = !IsChecked;
        }

        base.OnInteraction(args);
    }
}