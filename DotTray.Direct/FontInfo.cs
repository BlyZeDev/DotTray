namespace DotTray.Direct;

using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Represents a font description
/// </summary>
public sealed record FontInfo
{
    /// <summary>
    /// Segoe UI Emoji, 14 DIPs
    /// </summary>
    public static FontInfo Default => new FontInfo("Segoe UI Emoji", 14f);

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
    /// <see langword="true"/> if the font should be italic, otherwise <see langword="false"/>
    /// </summary>
    public bool Italic { get; init; } = false;

    /// <summary>
    /// The alignment of the text
    /// </summary>
    public TextAlign Align { get; init; } = TextAlign.Left;

    /// <summary>
    /// Initializes a new <see cref="FontInfo"/> using the default values
    /// </summary>
    public FontInfo() { }

    /// <summary>
    /// Initializes a new <see cref="FontInfo"/>
    /// </summary>
    /// <param name="family">The font family</param>
    /// <param name="size">The size of the font in DIPs</param>
    /// <param name="weight">The weight of the font, or <see langword="null"/> to use <see cref="FontWeight.Normal"/></param>
    /// <param name="italic"><see langword="true"/> if the font should be italic, otherwise <see langword="false"/></param>
    /// <param name="align">The alignment of the text</param>
    [SetsRequiredMembers]
    public FontInfo(string family, float size, FontWeight? weight = null, bool italic = false, TextAlign align = TextAlign.Left)
    {
        Family = family;
        Size = size;
        Weight = weight ?? FontWeight.Normal;
        Italic = italic;
        Align = align;
    }
}