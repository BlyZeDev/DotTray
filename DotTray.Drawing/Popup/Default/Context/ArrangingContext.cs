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
    public Dim WindowSize { get; }

    /// <summary>
    /// The original bounds, in window client coordinates, measured by <see cref="MenuItemBase.Measure(MeasuringContext)"/>
    /// </summary>
    /// <remarks>
    /// This is set immediately before each item's <see cref="MenuItemBase.Arrange(ArrangingContext)"/> is called
    /// </remarks>
    public Rect MeasuredItemBounds { get; internal set; }

    /// <summary>
    /// The bounds, in window client coordinates, assigned to the item thats about to be drawn
    /// </summary>
    /// <remarks>
    /// This is set immediately before each item's <see cref="MenuItemBase.Arrange(ArrangingContext)"/> is called
    /// </remarks>
    public Rect ItemBounds { get; internal set; }

    internal ArrangingContext(nint gdip, float scale, Dim windowSize) : base(gdip, scale)
    {
        WindowSize = windowSize;
    }
}