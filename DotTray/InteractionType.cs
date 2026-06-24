namespace DotTray;

using DotTray.Internal.Native;

/// <summary>
/// Represents the type of interaction with the <see cref="NotifyIcon"/> or its associated balloon notifications.
/// </summary>
public enum InteractionType : uint
{
    /// <summary>
    /// Occurs when the mouse pointer is moved over the notification icon.
    /// </summary>
    MouseMove = PInvoke.WM_MOUSEMOVE,

    /// <summary>
    /// Occurs when the left mouse button is pressed while the pointer is over the notification icon.
    /// </summary>
    LeftButtonDown = PInvoke.WM_LBUTTONDOWN,

    /// <summary>
    /// Occurs when the left mouse button is released while the pointer is over the notification icon.
    /// </summary>
    LeftButtonUp = PInvoke.WM_LBUTTONUP,

    /// <summary>
    /// Occurs when the left mouse button is double-clicked while the pointer is over the notification icon.
    /// </summary>
    LeftButtonDoubleClick = PInvoke.WM_LBUTTONDBLCLK,

    /// <summary>
    /// Occurs when the right mouse button is pressed while the pointer is over the notification icon.
    /// </summary>
    RightButtonDown = PInvoke.WM_RBUTTONDOWN,

    /// <summary>
    /// Occurs when the right mouse button is released while the pointer is over the notification icon.
    /// </summary>
    RightButtonUp = PInvoke.WM_RBUTTONUP,

    /// <summary>
    /// Occurs when the middle mouse button is pressed while the pointer is over the notification icon.
    /// </summary>
    MiddleButtonDown = PInvoke.WM_MBUTTONDOWN,

    /// <summary>
    /// Occurs when the middle mouse button is released while the pointer is over the notification icon.
    /// </summary>
    MiddleButtonUp = PInvoke.WM_MBUTTONUP,

    /// <summary>
    /// Occurs when the user requests a context menu, typically by right-clicking the icon or pressing the keyboard Menu key.
    /// </summary>
    ContextMenu = PInvoke.WM_CONTEXTMENU,

    /// <summary>
    /// Occurs when the notification icon is selected, either by a mouse click or by navigating to it via the keyboard.
    /// </summary>
    Select = PInvoke.NIN_SELECT,

    /// <summary>
    /// Occurs when the user selects the notification icon with the keyboard and presses the Spacebar or Enter key.
    /// </summary>
    KeySelect = PInvoke.NIN_KEYSELECT,

    /// <summary>
    /// Occurs when a balloon notification is successfully displayed to the user.
    /// </summary>
    BalloonShow = PInvoke.NIN_BALLOONSHOW,

    /// <summary>
    /// Occurs when a balloon notification is hidden, for example, when the icon is deleted or another balloon replaces it.
    /// </summary>
    /// <remarks>
    /// This does not occur if the balloon is clicked or times out.
    /// </remarks>
    BalloonHide = PInvoke.NIN_BALLOONHIDE,

    /// <summary>
    /// Occurs when a balloon notification is dismissed because it timed out or the user clicked the close (X) button.
    /// </summary>
    BalloonTimeout = PInvoke.NIN_BALLOONTIMEOUT,

    /// <summary>
    /// Occurs when the user clicks the body of the balloon notification.
    /// </summary>
    BalloonUserClick = PInvoke.NIN_BALLOONUSERCLICK,

    /// <summary>
    /// Occurs when the user hovers over the icon, indicating that a rich popup UI (flyout) should be displayed instead of a standard tooltip.
    /// </summary>
    PopupOpen = PInvoke.NIN_POPUPOPEN,

    /// <summary>
    /// Occurs when a rich popup UI (flyout) should be closed, typically because the user moved the cursor away or clicked elsewhere.
    /// </summary>
    PopupClose = PInvoke.NIN_POPUPCLOSE
}