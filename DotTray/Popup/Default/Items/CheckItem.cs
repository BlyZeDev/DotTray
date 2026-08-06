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
        var checkSize = Math.Max(8, baseSize.Height * 2 / 5);

        return new Size(baseSize.Width + CheckLeftPadding + checkSize + CheckGap, baseSize.Height);
    }

    /// <inheritdoc/>
    internal protected override Rectangle Arrange(ArrangingContext context)
    {
        var bounds = context.ItemBounds;

        var checkSize = Math.Max(8, bounds.Height * 2 / 5);
        var checkAreaWidth = CheckLeftPadding + checkSize + CheckGap;

        return bounds with
        {
            X = bounds.X + checkAreaWidth,
            Width = bounds.Width - checkAreaWidth
        };
    }

    /// <inheritdoc/>
    internal protected override void Draw(DrawingContext context)
    {
        base.Draw(context);

        var bounds = context.ItemBounds;
        var checkHeight = Math.Max(8, bounds.Height * 2 / 5);
        var checkWidth = checkHeight;
        var checkAreaWidth = CheckLeftPadding + checkWidth + CheckGap;

        var foreground = IsDisabled ? ForegroundDisabled : (isHovering ? ForegroundHover : Foreground);

        if (IsChecked)
        {
            var checkX = bounds.X - checkAreaWidth + CheckLeftPadding;
            var checkY = bounds.Y + (bounds.Height - checkHeight) / 2;
            var thickness = Math.Max(2, checkHeight / 4);

            ReadOnlySpan<Point> checkmark =
            [
                new Point(checkX, checkY + checkHeight * 45 / 100),
                new Point(checkX + checkWidth * 35 / 100, checkY + checkHeight * 85 / 100),
                new Point(checkX + checkWidth, checkY + checkHeight * 20 / 100),
                new Point(checkX + checkWidth - thickness, checkY + checkHeight * 20 / 100),
                new Point(checkX + checkWidth * 35 / 100, checkY + checkHeight * 85 / 100 - thickness),
                new Point(checkX, checkY + checkHeight * 45 / 100 + thickness)
            ];

            context.FillPolygon(foreground, checkmark);
        }
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