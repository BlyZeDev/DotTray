namespace DotTray;

/// <summary>
/// Represents a <see cref="NotifyIcon"/> separator
/// </summary>
public sealed record SeparatorItem : IMenuItem
{
    internal static readonly SeparatorItem Instance = new SeparatorItem();

    private SeparatorItem() { }
}