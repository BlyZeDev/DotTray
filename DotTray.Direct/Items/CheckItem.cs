namespace DotTray.Direct.Items;

using DotTray.Direct.Context;
using DotTray.Direct.Primitives;

/// <summary>
/// Represents a popup menu item that can be checked
/// </summary>
public class CheckItem : MenuItem
{
    private const int MinCheckHeight = 8;
    private const int MinCheckPadding = 5;
    private const int MinCheckGap = 5;
    private const float BaselineRatio = 0.78f;

    /// <summary>
    /// <see langword="true"/> if this instance is checked, otherwise <see langword="false"/>
    /// </summary>
    public virtual bool IsChecked
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

    /// <summary>
    /// Draws the indicator that is shown while this instance is checked, which is a checkmark by default
    /// </summary>
    /// <remarks>
    /// The box is sized from the item height and its bottom sits on the text baseline
    /// </remarks>
    /// <param name="context">Context for drawing this instance</param>
    /// <param name="box">The bounds of the indicator</param>
    protected virtual void DrawCheckedIndicator(DrawingContext context, Rect box) => context.DrawCheckmark(box, CurrentForeground);

    /// <summary>
    /// Draws the indicator that is shown while this instance is not checked, which is nothing by default
    /// </summary>
    /// <param name="context">Context for drawing this instance</param>
    /// <param name="box">The bounds of the indicator, same as for <see cref="DrawCheckedIndicator"/></param>
    protected virtual void DrawUncheckedIndicator(DrawingContext context, Rect box) { }

    /// <inheritdoc/>
    /// <remarks>
    /// Adds the space of the indicator in front of the text, which is reserved even while unchecked
    /// </remarks>
    protected override Dim MeasureContent(MeasuringContext context)
    {
        var content = base.MeasureContent(context);

        GetCheckMetrics(content.Height, out var width, out var height, out var padding, out var gap);
        return new Dim(padding + width + gap + content.Width, Math.Max(content.Height, height));
    }

    /// <inheritdoc/>
    protected override void DrawContent(DrawingContext context, Rect bounds)
    {
        GetCheckMetrics(bounds.Height, out var width, out var height, out var padding, out var gap);

        var baseline = bounds.Y + (int)MathF.Round(bounds.Height * BaselineRatio);
        var box = new Rect(bounds.X + padding, baseline - height, width, height);

        if (IsChecked) DrawCheckedIndicator(context, box);
        else DrawUncheckedIndicator(context, box);

        var offset = padding + width + gap;
        base.DrawContent(context, new Rect(bounds.X + offset, bounds.Y, bounds.Width - offset, bounds.Height));
    }

    /// <inheritdoc/>
    internal protected override void OnInteraction(ItemInteractedEventArgs args)
    {
        if (ShouldToggle(args)) IsChecked = !IsChecked;

        base.OnInteraction(args);
    }

    private static void GetCheckMetrics(int itemHeight, out int width, out int height, out int padding, out int gap)
    {
        height = Math.Max(MinCheckHeight, itemHeight * 13 / 25);
        width = height * 4 / 3;
        padding = Math.Max(MinCheckPadding, itemHeight * 6 / 25);
        gap = Math.Max(MinCheckGap, itemHeight * 6 / 25);
    }
}