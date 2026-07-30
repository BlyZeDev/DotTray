namespace DotTray.Popup.Default;

using DotTray.Popup.Default.Context;
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
    /// Fires whenever the user interacts with the <see cref="MenuItemBase"/>
    /// </summary>
    /// <remarks>
    /// <b>Note:</b> This action is fired on the <see cref="NotifyIcon"/>'s background STA thread
    /// </remarks>
    public Action<ItemInteractedEventArgs>? Interacted { get; set; }

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
    /// Called after every item in the popup has been measured, to determine this item's final position and width
    /// within the row it was offered
    /// </summary>
    /// <remarks>
    /// <b>Important:</b> Only <see cref="Rectangle.X"/> and <see cref="Rectangle.Width"/> of the returned <see cref="Rectangle"/> are respected.<br/><br/>
    /// The default implementation returns <see cref="ArrangingContext.ItemBounds"/> unchanged, so items fill the
    /// full width of the popup by default.<br/>
    /// Override <see cref="Arrange(ArrangingContext)"/> and return <see cref="ArrangingContext.MeasuredItemBounds"/> instead to opt this item out of arranging entirely
    /// </remarks>
    /// <param name="context">Context for arranging this instance</param>
    /// <returns><see cref="Rectangle"/></returns>
    internal protected virtual Rectangle Arrange(ArrangingContext context) => context.ItemBounds;

    /// <summary>
    /// Called when this instance needs to be drawn
    /// </summary>
    /// <param name="context">Context for drawing this instance</param>
    internal protected abstract void Draw(DrawingContext context);

    /// <summary>
    /// Called when this instance is interacted and calls <see cref="Interacted"/>
    /// </summary>
    /// <remarks>
    /// To ignore interaction, override <see cref="OnInteraction(ItemInteractedEventArgs)"/> without calling the <see langword="base"/> implementation
    /// </remarks>
    /// <param name="args">The interaction that occurred</param>
    internal protected virtual void OnInteraction(ItemInteractedEventArgs args) => Interacted?.Invoke(args);
}