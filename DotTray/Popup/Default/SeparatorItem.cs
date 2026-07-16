namespace DotTray.Popup.Default;

using DotTray.Popup.Default.Coloring;
using DotTray.Primitives;

/// <summary>
/// Represents a basic popup separator item
/// </summary>
public class SeparatorItem : MenuItemBase
{
    /// <inheritdoc/>
    internal protected override Size Measure(MeasuringContext context)
    {
        return new Size(250, 10);
    }

    /// <inheritdoc/>
    internal protected override void Draw(DrawingContext context)
    {
        context.Fill(SolidColor.Black);
    }
}