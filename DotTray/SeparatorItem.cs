namespace DotTray;

/// <summary>
/// Represents a <see cref="NotifyIcon"/> separator
/// </summary>
public sealed class SeparatorItem : IMenuItem
{
    private static readonly TrayColor DefaultLineColor = new TrayColor(255, 255, 255);
    private static readonly float DefaultLineThickness = 1f;

    /// <summary>
    /// The line color of this separator
    /// </summary>
    public TrayColor LineColor { get; set; } = DefaultLineColor;

    /// <summary>
    /// The line thickness of this separator
    /// </summary>
    public float LineThickness { get; set; } = DefaultLineThickness;
}