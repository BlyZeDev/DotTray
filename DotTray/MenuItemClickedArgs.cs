namespace DotTray;

/// <summary>
/// Represents data that is created if a <see cref="DotTray.MenuItem"/> is interacted with
/// </summary>
public sealed record MenuItemClickedArgs
{
    /// <summary>
    /// The icon where the click originated from
    /// </summary>
    public required NotifyIcon Icon { get; init; }

    /// <summary>
    /// The menu item that is interacted with
    /// </summary>
    public required MenuItem MenuItem { get; init; }
}