namespace DotTray.Popup.Default;

using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Contains information about a font
/// </summary>
public readonly record struct FontInfo
{
    /// <summary>
    /// The font family name
    /// </summary>
    /// <remarks>
    /// For example <i>Segoe UI Emoji</i>
    /// </remarks>
    public required readonly string FontFamilyName { get; init; }
    /// <summary>
    /// The font size
    /// </summary>
    /// <remarks>
    /// For example <i>20f</i>
    /// </remarks>
    public required readonly float Size { get; init; }

    /// <summary>
    /// Initializes a new <see cref="FontInfo"/>
    /// </summary>
    /// <param name="fontFamilyName">The font family name</param>
    /// <param name="size">The font size</param>
    [SetsRequiredMembers]
    public FontInfo(string fontFamilyName, float size)
    {
        FontFamilyName = fontFamilyName;
        Size = size;
    }
}