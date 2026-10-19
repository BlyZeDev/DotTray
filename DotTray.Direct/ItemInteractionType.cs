namespace DotTray.Direct;

using DotTray.Direct.Items;

/// <summary>
/// Represents the kind of interaction that happened with a <see cref="MenuItemBase"/>
/// </summary>
public enum ItemInteractionType
{
    /// <summary>The mouse entered the item</summary>
    MouseEnter,

    /// <summary>The mouse left the item</summary>
    MouseLeave,

    /// <summary>The left mouse button was pressed on the item</summary>
    MouseLeftDown,

    /// <summary>The left mouse button was released on the item</summary>
    MouseLeftUp,

    /// <summary>The right mouse button was pressed on the item</summary>
    MouseRightDown,

    /// <summary>The right mouse button was released on the item</summary>
    MouseRightUp,

    /// <summary>The item got focused by the keyboard navigation</summary>
    KeyboardFocus,

    /// <summary>The item lost the focus of the keyboard navigation</summary>
    KeyboardBlur,

    /// <summary>The item was activated by the keyboard (Enter)</summary>
    KeyboardActivate
}
