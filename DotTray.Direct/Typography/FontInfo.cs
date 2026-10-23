namespace DotTray.Direct.Typography;

/// <summary>
/// Represents a font description
/// </summary>
public sealed record FontInfo
{
    /// <summary>
    /// Segoe UI Emoji, 16 DIPs
    /// </summary>
    public static FontInfo Default => new FontInfo
    {
        Family = "Segoe UI Emoji",
        Size = 16f
    };

    /// <summary>
    /// The name of the font family, eg. Segoe UI Emoji
    /// </summary>
    public required string Family { get; init; }

    /// <summary>
    /// The size in DIPs
    /// </summary>
    public required float Size { get; init; }

    /// <summary>
    /// The weight of the font
    /// </summary>
    public FontWeight Weight { get; init; } = FontWeight.Normal;

    /// <summary>
    /// The style of the font
    /// </summary>
    public FontStyle Style { get; init; } = FontStyle.Normal;

    /// <summary>
    /// The stretching of the font
    /// </summary>
    public FontStretch Stretch { get; init; } = FontStretch.Normal;

    /// <summary>
    /// The alignment of the text
    /// </summary>
    public TextAlign Align { get; init; } = TextAlign.Left;
}