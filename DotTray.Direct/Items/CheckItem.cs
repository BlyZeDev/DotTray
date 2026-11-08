namespace DotTray.Direct.Items;

using DotTray.Direct.Context;
using DotTray.Direct.Primitives;

/// <summary>
/// Represents a popup menu item that can be checked
/// </summary>
public class CheckItem : MenuItem
{
    private const int MinCheckHeight = 12;
    private const int MinCheckPadding = 5;
    private const int MinCheckGap = 5;
    private const float BaselineRatio = 0.8f;

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

    /// <summary>
    /// Default configuration for <see cref="CheckItem"/>
    /// </summary>
    public CheckItem() { }

    /// <summary>
    /// Determines whether <paramref name="args"/> should toggle <see cref="IsChecked"/>
    /// </summary>
    /// <remarks>
    /// By default, this is <see langword="true"/> for <see cref="ItemInteractionType.MouseLeftUp"/> and <see cref="ItemInteractionType.KeyboardActivate"/>.<br/>
    /// Override to change which interactions toggle the item, or return <see langword="false"/> to manage <see cref="IsChecked"/> manually
    /// </remarks>
    /// <param name="args">The interaction that occurred</param>
    /// <returns><see langword="true"/> to toggle <see cref="IsChecked"/>, otherwise <see langword="false"/></returns>
    protected virtual bool ShouldToggle(ItemInteractedEventArgs args) => args.Type is ItemInteractionType.MouseLeftUp or ItemInteractionType.KeyboardActivate;

    /// <inheritdoc/>
    protected override Dim MeasureLeading(MeasuringContext context, Dim contentSize)
    {
        GetCheckMetrics(contentSize.Height, out var width, out var height, out var padding, out var gap);
        return new Dim(padding + width + gap, height);
    }

    /// <inheritdoc/>
    protected override void DrawLeading(DrawingContext context, Rect bounds)
    {
        if (!IsChecked) return;

        GetCheckMetrics(bounds.Height, out var width, out var height, out var padding, out _);

        var baseline = bounds.Y + (int)MathF.Round(bounds.Height * BaselineRatio);

        var checkBounds = new Rect(
            bounds.X + padding,
            baseline - height,
            width,
            height);

        context.DrawCheckmark(checkBounds, CurrentForeground);
    }

    /// <inheritdoc/>
    internal protected override void OnInteraction(ItemInteractedEventArgs args)
    {
        if (ShouldToggle(args)) IsChecked = !IsChecked;

        base.OnInteraction(args);
    }

    private static void GetCheckMetrics(int itemHeight, out int width, out int height, out int padding, out int gap)
    {
        height = Math.Max(MinCheckHeight, itemHeight * 3 / 5);
        width = height * 4 / 3;
        padding = Math.Max(MinCheckPadding, height * 2 / 5);
        gap = Math.Max(MinCheckGap, height * 2 / 5);
    }
}