namespace DotTray.Popup.Default;

using DotTray.Primitives;
using System;

/// <summary>
/// Represents the base for a popup menu item
/// </summary>
public abstract class MenuItemBase
{
    internal Size DrawBox { get; set; }
    internal event Action? Updated;

    /// <summary>
    /// Invokes redrawing this instance when called
    /// </summary>
    protected void Update() => Updated?.Invoke();

    /// <summary>
    /// Called when this instance needs to be measured
    /// </summary>
    /// <param name="context">Context for measuring this instance</param>
    /// <returns><see cref="Size"/></returns>
    internal protected abstract Size Measure(MeasuringContext context);

    /// <summary>
    /// Called when this instance needs to be drawn
    /// </summary>
    /// <param name="context">Context for drawing this instance</param>
    internal protected abstract void Draw(DrawingContext context);
}