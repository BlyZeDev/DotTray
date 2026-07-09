namespace DotTray.Popup.Default;

using DotTray.Popup.Default.Abstract;
using System.Drawing;

/// <summary>
/// Represents a basic popup menu item
/// </summary>
public class MenuItem : MenuItemBase
{
    /// <inheritdoc/>
    internal protected override SizeF Measure(MeasuringContext context)
    {
        return new SizeF(250, 25);
    }

    /// <inheritdoc/>
    internal protected override void Draw(DrawingContext context)
    {
        context.Fill(TrayColor.Random());
    }
}