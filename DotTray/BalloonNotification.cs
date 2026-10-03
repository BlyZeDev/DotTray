namespace DotTray;

using DotTray.Internal.Models;
using System;

/// <summary>
/// Represents a balloon message that can be shown using <see cref="NotifyIcon"/>
/// </summary>
public sealed record BalloonNotification
{
    /// <summary>
    /// The title of the notification
    /// </summary>
    /// <remarks>
    /// This is truncated to fit into the allowed Windows balloon notification title character length
    /// </remarks>
    public required string Title
    {
        get;
        init => field = value.Length > NOTIFYICONDATA.SZINFOTITLE_LENGTH ? value[..NOTIFYICONDATA.SZINFOTITLE_LENGTH] : value;
    }

    /// <summary>
    /// The message of the notification
    /// </summary>
    /// <remarks>
    /// This is truncated to fit into the allowed Windows balloon notification message character length
    /// </remarks>
    public required string Message
    {
        get;
        init => field = value.Length > NOTIFYICONDATA.SZINFO_LENGTH ? value[..NOTIFYICONDATA.SZINFO_LENGTH] : value;
    }

    /// <summary>
    /// The icon of the notification
    /// </summary>
    public BalloonNotificationIcon Icon
    {
        get;
        init
        {
            if (Enum.IsDefined(value)) field = value;
        }
    } = BalloonNotificationIcon.None;

    /// <summary>
    /// <see langword="true"/> if no sound should be played, otherwise <see langword="false"/>
    /// </summary>
    /// <remarks>
    /// The default is <see langword="false"/>
    /// </remarks>
    public bool NoSound { get; init; } = false;
}