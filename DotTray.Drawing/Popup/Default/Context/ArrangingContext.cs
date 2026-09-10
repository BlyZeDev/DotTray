namespace DotTray.Popup.Default.Context;

using DotTray.Popup.Default;
using DotTray.Primitives;

/// <summary>
/// Includes data for arranging <see cref="MenuItemBase"/> instances
/// </summary>
public sealed class ArrangingContext : Context
{
    /// <summary>
    /// The size of the window that contains this item
    /// </summary>
    public Size WindowSize { get; }

    /// <summary>
    /// The original bounds, in window client coordinates, measured by <see cref="MenuItemBase.Measure(MeasuringContext)"/>
    /// </summary>
    /// <remarks>
    /// This is set immediately before each item's <see cref="MenuItemBase.Arrange(ArrangingContext)"/> is called
    /// </remarks>
    public Rectangle MeasuredItemBounds { get; internal set; }

    /// <summary>
    /// The bounds, in window client coordinates, assigned to the item thats about to be drawn
    /// </summary>
    /// <remarks>
    /// This is set immediately before each item's <see cref="MenuItemBase.Arrange(ArrangingContext)"/> is called
    /// </remarks>
    public Rectangle ItemBounds { get; internal set; }

    internal ArrangingContext(nint gdip, float scale, Size windowSize) : base(gdip, scale)
    {
        WindowSize = windowSize;
    }
}