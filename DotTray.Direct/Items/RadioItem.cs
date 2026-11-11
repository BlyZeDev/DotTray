namespace DotTray.Direct.Items;

using DotTray.Direct.Context;
using DotTray.Direct.Primitives;

/// <summary>
/// Represents a popup menu item that is part of a <see cref="RadioItemGroup"/> where only one item can be selected
/// </summary>
/// <remarks>
/// Interacting with an unselected item selects it and unchecks the previously selected item of the same <see cref="Group"/>.<br/>
/// Interacting with the selected item does nothing, use <see cref="RadioItemGroup.ClearSelection"/> to clear the selection in code
/// </remarks>
public class RadioItem : CheckItem
{
    private const float OutlineRatio = 0.1f;
    private const float MinOutline = 1f;
    private const float HoleRatio = 0.4f;

    /// <summary>
    /// The group this instance belongs to, or <see langword="null"/> if it is not part of any group
    /// </summary>
    /// <remarks>
    /// Setting this removes the item from its previous group.<br/>
    /// If the item is already checked when it joins a group, it becomes the selected item of that group
    /// </remarks>
    public RadioItemGroup? Group
    {
        get;
        set
        {
            if (ReferenceEquals(field, value)) return;

            var previous = field;
            field = value;

            previous?.Detach(this);
            value?.Attach(this);
        }
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Setting this to <see langword="true"/> unchecks every other item of the <see cref="Group"/>
    /// </remarks>
    public override bool IsChecked
    {
        get => base.IsChecked;
        set
        {
            if (Group is null)
            {
                base.IsChecked = value;
                return;
            }

            if (value) Group.Select(this);
            else Group.Deselect(this);
        }
    }

    /// <summary>
    /// Default configuration for <see cref="RadioItem"/>
    /// </summary>
    public RadioItem() { }

    /// <inheritdoc/>
    protected override bool ShouldToggle(ItemInteractedEventArgs args) => !IsChecked && base.ShouldToggle(args);

    /// <summary>
    /// Draws the indicator that is shown while this instance is checked, which is a filled dot by default
    /// </summary>
    /// <param name="context">Context for drawing this instance</param>
    /// <param name="box">The bounds of the indicator, the dot is centered inside</param>
    protected override void DrawCheckedIndicator(DrawingContext context, Rect box) => DrawCircle(context, box, (1f - HoleRatio) / 2f);

    /// <summary>
    /// Draws the indicator that is shown while this instance is not checked, which is a thin circle outline by default
    /// </summary>
    /// <param name="context">Context for drawing this instance</param>
    /// <param name="box">The bounds of the indicator, the circle is centered inside</param>
    protected override void DrawUncheckedIndicator(DrawingContext context, Rect box) => DrawCircle(context, box, OutlineRatio);

    internal void ApplyChecked(bool value) => base.IsChecked = value;

    private void DrawCircle(DrawingContext context, Rect box, float thicknessRatio)
    {
        var diameter = box.Height;
        var thickness = MathF.Max(MinOutline, diameter * thicknessRatio);

        var inset = thickness / 2f;

        context.DrawEllipse(new RectF(
            box.X + (box.Width - diameter) / 2f + inset,
            box.Y + inset,
            diameter - thickness,
            diameter - thickness), CurrentForeground, thickness);
    }
}