namespace DotTray.Popup.Default;

/// <summary>
/// Marks a <see cref="MenuItemBase"/> as being able to open a submenu
/// </summary>
public interface ISubmenu
{
    /// <summary>
    /// The items of the submenu opened by this instance
    /// </summary>
    public MenuItemCollection Items { get; }

    /// <summary>
    /// Determines whether <paramref name="args"/> should open the submenu
    /// </summary>
    /// <param name="args">The interaction that occurred on this item</param>
    /// <returns><see langword="true"/> if the submenu should open, otherwise <see langword="false"/></returns>
    bool ShouldOpen(ItemInteractedEventArgs args);
}