namespace DotTray.Popup.Default;

/// <summary>
/// Describes the kind of interaction that occurred on a <see cref="MenuItemBase"/>
/// </summary>
public enum ItemInteractionType
{
    /// <summary>
    /// The cursor started hovering the item
    /// </summary>
    MouseEnter,
    /// <summary>
    /// The cursor stopped hovering the item
    /// </summary>
    MouseLeave,
    /// <summary>
    /// The left mouse button was pressed down while over the item
    /// </summary>
    MouseLeftDown,
    /// <summary>
    /// The left mouse button was released while over the item
    /// </summary>
    MouseLeftUp,
    /// <summary>
    /// The right mouse button was pressed down while over the item
    /// </summary>
    MouseRightDown,
    /// <summary>
    /// The right mouse button was released while over the item
    /// </summary>
    MouseRightUp
}