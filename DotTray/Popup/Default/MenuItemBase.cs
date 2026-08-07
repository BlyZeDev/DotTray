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
    /// <see langword="true"/> if this instance is purely visual and therefore should not be included in any hittesting, otherwise <see langword="false"/>
    /// </summary>
    internal protected virtual bool IgnoreHitTest { get; } = false;

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
    /// Called when the popup window hosting this item is created and about to be shown
    /// </summary>
    /// <remarks>
    /// Since items are created once and reused across popup menu instances, this can be used to reset the items state
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
    /// Called when this instance is interacted and calls <see cref="Interacted"/>
    /// </summary>
    /// <remarks>
    /// To ignore interaction, override <see cref="OnInteraction(ItemInteractedEventArgs)"/> without calling the <see langword="base"/> implementation
    /// </remarks>
    /// <param name="args">The interaction that occurred</param>
    internal protected virtual void OnInteraction(ItemInteractedEventArgs args)
    {
        if (IgnoreHitTest) return;

        Interacted?.Invoke(args);
    }

    internal bool ShouldOpenSubmenu(ItemInteractionType type) => !IgnoreHitTest && !SubmenuItems.IsEmpty && type is ItemInteractionType.MouseEnter or ItemInteractionType.KeyboardActivate;
}