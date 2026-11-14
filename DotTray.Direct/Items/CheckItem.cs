namespace DotTray.Direct.Items;

using DotTray.Direct.Context;
using DotTray.Direct.Primitives;
using DotTray.Direct.Typography;

/// <summary>
/// Represents a popup menu item that can be checked
/// </summary>
public class CheckItem : MenuItem
{
    private const float WidthRatio = 4f / 3f;
    private const float PaddingRatio = 0.5f;
    private const float GapRatio = 0.5f;

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
    /// The box is as tall as the capital letters of the text and its bottom sits on the text baseline
    /// </remarks>
    /// <param name="context">Context for drawing this instance</param>
    /// <param name="box">The bounds of the indicator</param>
    protected virtual void DrawCheckedIndicator(DrawingContext context, RectF box)
        => context.DrawCheckmark(box, CurrentForeground, context.GetFontMetrics(TextInfo).StrokeThicknessOrEstimate);

    /// <summary>
    /// Draws the indicator that is shown while this instance is not checked, which is nothing by default
    /// </summary>
    /// <param name="context">Context for drawing this instance</param>
    /// <param name="box">The bounds of the indicator, same as for <see cref="DrawCheckedIndicator"/></param>
    protected virtual void DrawUncheckedIndicator(DrawingContext context, RectF box) { }

    /// <inheritdoc/>
    /// <remarks>
    /// Adds the space of the indicator in front of the text, which is reserved even while unchecked
    /// </remarks>
    protected override Dim MeasureContent(MeasuringContext context)
    {
        var content = base.MeasureContent(context);

        return new Dim(GetIndicatorOffset(context.GetFontMetrics(TextInfo)) + content.Width, content.Height);
    }

    /// <inheritdoc/>
    protected override void DrawContent(DrawingContext context, Rect bounds)
    {
        var metrics = context.GetFontMetrics(TextInfo);
        GetIndicatorMetrics(metrics, out var width, out var height, out var padding, out _);

        var baseline = bounds.Y + metrics.BaselineIn(bounds.Height);
        var box = new RectF(bounds.X + padding, baseline - height, width, height);

        if (IsChecked) DrawCheckedIndicator(context, box);
        else DrawUncheckedIndicator(context, box);

        var offset = GetIndicatorOffset(metrics);
        base.DrawContent(context, new Rect(bounds.X + offset, bounds.Y, bounds.Width - offset, bounds.Height));
    }

    /// <inheritdoc/>
    internal protected override void OnInteraction(ItemInteractedEventArgs args)
    {
        if (ShouldToggle(args)) IsChecked = !IsChecked;

        base.OnInteraction(args);
    }

    private static int GetIndicatorOffset(FontMetrics metrics)
    {
        GetIndicatorMetrics(metrics, out var width, out _, out var padding, out var gap);
        return (int)MathF.Ceiling(padding + width + gap);
    }

    private static void GetIndicatorMetrics(FontMetrics metrics, out float width, out float height, out float padding, out float gap)
    {
        height = metrics.CapHeightOrEstimate;
        width = height * WidthRatio;
        padding = height * PaddingRatio;
        gap = height * GapRatio;
    }
}