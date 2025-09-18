namespace DotTray;

/// <summary>
/// Represents a <see cref="NotifyIcon"/> separator
/// </summary>
public sealed record SeparatorItem : MenuItem
{
    /// <summary>
    /// The shared <see cref="SeparatorItem"/> instance to use
    /// </summary>
    public static readonly SeparatorItem Instance = new SeparatorItem();

    private SeparatorItem() { }
}