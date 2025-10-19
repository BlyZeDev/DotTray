namespace DotTray;

using DotTray.Internal.Native;

/// <summary>
/// Represents an icon that can be shown in a <see cref="BalloonNotification"/>
/// </summary>
public enum BalloonNotificationIcon : uint
{
    /// <summary>
    /// No icon is shown
    /// </summary>
    None = PInvoke.NIIF_NONE,
    /// <summary>
    /// An information icon is shown
    /// </summary>
    Info = PInvoke.NIIF_INFO,
    /// <summary>
    /// A warning icon is shown
    /// </summary>
    Warning = PInvoke.NIIF_WARNING,
    /// <summary>
    /// An error icon is shown
    /// </summary>
    Error = PInvoke.NIIF_ERROR,
    /// <summary>
    /// The icon of the <see cref="NotifyIcon"/> is shown
    /// </summary>
    User = PInvoke.NIIF_USER
}