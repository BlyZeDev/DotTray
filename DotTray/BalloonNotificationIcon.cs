namespace DotTray;

using DotTray.Internal;

/// <summary>
/// Represents an icon that can be shown in a <see cref="BalloonNotification"/>
/// </summary>
public enum BalloonNotificationIcon : uint
{
    /// <summary>
    /// No icon is shown
    /// </summary>
    None = Native.NIIF_NONE,
    /// <summary>
    /// An information icon is shown
    /// </summary>
    Info = Native.NIIF_INFO,
    /// <summary>
    /// A warning icon is shown
    /// </summary>
    Warning = Native.NIIF_WARNING,
    /// <summary>
    /// An error icon is shown
    /// </summary>
    Error = Native.NIIF_ERROR
}