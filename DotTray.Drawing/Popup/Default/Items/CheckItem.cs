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

    private Rect checkBounds;

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
    internal protected override Dim Measure(MeasuringContext context)
    {
        var baseSize = base.Measure(context);
        var checkSize = Math.Max(10, baseSize.Height / 2);

        return baseSize with { Width = baseSize.Width + CheckLeftPadding + checkSize + CheckGap };
    }

    /// <inheritdoc/>
    internal protected override Rect Arrange(ArrangingContext context)
    {
        var itemBounds = context.ItemBounds;

        var checkSize = Math.Max(10, itemBounds.Height / 2);
        var checkAreaWidth = CheckLeftPadding + checkSize + CheckGap;

        checkBounds = new Rect(itemBounds.X + CheckLeftPadding, itemBounds.Y + (itemBounds.Height - checkSize) / 2, checkSize, checkSize);

        return itemBounds with { X = itemBounds.X + checkAreaWidth, Width = itemBounds.Width - checkAreaWidth };
    }

    /// <inheritdoc/>
    internal protected override void Draw(DrawingContext context)
    {
        base.Draw(context);

        var foreground = IsDisabled ? ForegroundDisabled : (isHovering ? ForegroundHover : Foreground);
        var background = IsDisabled ? BackgroundDisabled : (isHovering ? BackgroundHover : Background);

        var gutter = new Rect(checkBounds.X - CheckLeftPadding, context.ItemBounds.Y, checkBounds.Width + CheckLeftPadding + CheckGap, context.ItemBounds.Height);
        context.FillRect(gutter, background);

        if (!IsChecked) return;

        context.DrawCheckmark(checkBounds, foreground);
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