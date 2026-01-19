namespace DotTray;

using System;

/// <summary>
/// Represents a badge that can be shown on top of a <see cref="NotifyIcon"/>
/// </summary>
public sealed record NotifyIconBadge
{
    /// <summary>
    /// The default values for a <see cref="NotifyIconBadge"/> instance
    /// </summary>
    public static readonly NotifyIconBadge Default = new NotifyIconBadge();

    /// <summary>
    /// The background color of the badge
    /// </summary>
    public TrayColor BackgroundColor { get; init; } = TrayColor.Red;

    /// <summary>
    /// The border radius of the badge
    /// </summary>
    /// <remarks>
    /// 0 = rectangle<br/>
    /// 1 = circle
    /// </remarks>
    public float BorderRadius
    {
        get => field;
        init => field = Math.Clamp(value, 0f, 1f);
    } = 1f;

    /// <summary>
    /// The position of the badge relative to the icon
    /// </summary>
    public NotifyIconBadgePosition Position
    {
        get => field;
        init
        {
            if (!Enum.IsDefined(value)) return;

            field = value;
        }
    } = NotifyIconBadgePosition.TopRight;
}