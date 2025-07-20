namespace DotTray;

using DotTray.Internal.Win32;
using System;

/// <summary>
/// Represents a balloon message that can be send using <see cref="NotifyIcon"/>
/// </summary>
public sealed record BalloonNotification
{
    private readonly string _title = "";
    private readonly string _message = "";
    private readonly BalloonNotificationIcon _icon = BalloonNotificationIcon.None;

    /// <summary>
    /// The title of the notification
    /// </summary>
    /// <remarks>
    /// This is truncated to fit into the allowed Windows balloon notification title character length
    /// </remarks>
    public required string Title
    {
        get => _title;
        init => _title = value.Length > NOTIFYICONDATA.SZINFOTITLE_LENGTH ? value[..NOTIFYICONDATA.SZINFOTITLE_LENGTH] : value;
    }

    /// <summary>
    /// The message of the notification
    /// </summary>
    /// <remarks>
    /// This is truncated to fit into the allowed Windows balloon notification message character length
    /// </remarks>
    public required string Message
    {
        get => _message;
        init => _message = value.Length > NOTIFYICONDATA.SZINFO_LENGTH ? value[..NOTIFYICONDATA.SZINFO_LENGTH] : value;
    }

    /// <summary>
    /// The icon of the notification
    /// </summary>
    public required BalloonNotificationIcon Icon
    {
        get => _icon;
        init
        {
            if (Enum.IsDefined(value)) _icon = value;
        }
    }
}