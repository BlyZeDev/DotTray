namespace DotTray.Default;

/// <summary>
/// Represents a default Win32 separator item
/// </summary>
public sealed class SeparatorItem : Win32Item
{
    /// <summary>
    /// Shared instance of a separator item
    /// </summary>
    public static readonly SeparatorItem Instance = new SeparatorItem();

    private SeparatorItem() { }
}