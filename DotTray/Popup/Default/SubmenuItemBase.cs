namespace DotTray.Popup.Default;

/// <summary>
/// Represents the base for a popup menu item that can open a submenu
/// </summary>
public abstract class SubmenuItemBase : MenuItemBase
{
    /// <summary>
    /// The items of the submenu opened by this instance
    /// </summary>
    public MenuItemCollection Items { get; } = [];

    /// <summary>
    /// Determines whether <paramref name="args"/> should open the submenu
    /// </summary>
    /// <remarks>
    /// Override this if you need custom opening logic
    /// </remarks>
    /// <param name="args">The interaction that occurred on this item</param>
    /// <returns><see langword="true"/> if the submenu should open, otherwise <see langword="false"/></returns>
    internal protected virtual bool ShouldOpen(ItemInteractedEventArgs args) => !Items.IsEmpty && args.Type is ItemInteractionType.MouseEnter;
}