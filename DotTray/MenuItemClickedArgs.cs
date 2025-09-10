namespace DotTray;

/// <summary>
/// Represents data that is created if <see cref="MenuItem.Click"/> is invoked
/// </summary>
public sealed record MenuItemClickedArgs
{
    /// <summary>
    /// The icon where the click originated from
    /// </summary>
    public required NotifyIcon Icon { get; init; }
    /// <summary>
    /// The selected menu item
    /// </summary>
    public required MenuItem MenuItem { get; init; }
}