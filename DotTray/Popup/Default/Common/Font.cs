namespace DotTray.Popup.Default.Common;

/// <summary>
/// Contains information about a font
/// </summary>
public readonly record struct Font
{
    /// <summary>
    /// The font family name
    /// </summary>
    /// <remarks>
    /// For example <i>Segoe UI Emoji</i>
    /// </remarks>
    public required readonly string FontFamily { get; init; }
    /// <summary>
    /// The font size
    /// </summary>
    /// <remarks>
    /// For example <i>20f</i>
    /// </remarks>
    public required readonly float Size { get; init; }
}