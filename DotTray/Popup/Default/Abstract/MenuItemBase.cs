namespace DotTray.Popup.Default.Abstract;

using DotTray.Internal.Win32;
using System;
using System.Drawing;

/// <summary>
/// Represents the base for a popup menu item
/// </summary>
public abstract class MenuItemBase
{
    internal RECTF DrawBox { get; set; }
    internal event Action? Updated;

    /// <summary>
    /// Invokes redrawing this instance when called
    /// </summary>
    protected void Update() => Updated?.Invoke();

    /// <summary>
    /// Called when this instance needs to be measured
    /// </summary>
    /// <param name="context">Context for measuring this instance</param>
    /// <returns><see cref="SizeF"/></returns>
    internal protected abstract SizeF Measure(MeasuringContext context);

    /// <summary>
    /// Called when this instance needs to be drawn
    /// </summary>
    /// <param name="context">Context for drawing this instance</param>
    internal protected abstract void Draw(DrawingContext context);
}