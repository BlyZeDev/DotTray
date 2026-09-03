namespace DotTray.Popup.Default;

using DotTray.Popup.Default.Context;
using DotTray.Primitives;
using System;

/// <summary>
/// Represents the base for a popup menu item
/// </summary>
public abstract class MenuItemBase
{
    internal Rectangle HitBounds { get; set; }
    internal Rectangle ContentBounds { get; set; }

    internal event Action? Updated;

    /// <summary>
    /// The items of the submenu opened by this instance
    /// </summary>
    internal protected virtual MenuItemCollection SubmenuItems { get; } = [];

    /// <summary>
    /// <see langword="true"/> if this instance is purely visual and therefore should not be included in any interactions, otherwise <see langword="false"/>
    /// </summary>
    internal protected virtual bool IgnoreInteraction { get; } = false;

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

    internal void RaiseInteraction(ItemInteractedEventArgs args)
    {
        if (IgnoreInteraction) return;

        OnInteraction(args);
        Interacted?.Invoke(args);
    }

    /// <summary>
    /// Called when the popup window hosting this item is created and about to be shown
    /// </summary>
    /// <remarks>
    /// Since items are created once and reused across popup menu instances, this could be used to reset the items state for example
    /// </remarks>
    internal protected virtual void Initialize() { }

    /// <summary>
    /// Called when this instance needs to be measured
    /// </summary>
    /// <param name="context">Context for measuring this instance</param>
    /// <returns><see cref="Size"/></returns>
    internal protected abstract Size Measure(MeasuringContext context);

    /// <summary>
    /// Called after every item in the popup has been measured, to determine the bounds this item
    /// should draw its content into
    /// </summary>
    /// <remarks>
    /// The default implementation returns <see cref="ArrangingContext.ItemBounds"/> unchanged, so items
    /// draw across the full width of the popup by default.<br/>
    /// Override to return a narrower <see cref="Rectangle"/> and reserve space for extra content
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
    /// Called when this instance is interacted with
    /// </summary>
    /// <remarks>
    /// This will not be called if <see cref="IgnoreInteraction"/> is <see langword="true"/>
    /// </remarks>
    /// <param name="args">The interaction that occurred</param>
    internal protected virtual void OnInteraction(ItemInteractedEventArgs args) { }

    /// <summary>
    /// Called when the popup window hosting this item is being torn down
    /// </summary>
    /// <remarks>
    /// Since items are created once and reused across popup menu instances, this could be used to clean up resources for example
    /// </remarks>
    internal protected virtual void Cleanup() { }

    internal bool CanOpenSubmenu(ItemInteractionType type) => !IgnoreInteraction && !SubmenuItems.IsEmpty && type is ItemInteractionType.MouseEnter or ItemInteractionType.KeyboardActivate;
}