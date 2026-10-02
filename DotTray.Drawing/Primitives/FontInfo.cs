namespace DotTray.Drawing.Primitives;

using System.Diagnostics.CodeAnalysis;
using System.Drawing;

/// <summary>
/// Represents information about a font
/// </summary>
public sealed record FontInfo
{
    /// <summary>
    /// The font family name, e.g. <i>Segoe UI Emoji</i>
    /// </summary>
    public required string FontFamily { get; init; }

    /// <summary>
    /// The size of the font
    /// </summary>
    /// <remarks>
    /// This value is clamped to be equal or higher to <i><see cref="float.Epsilon"/></i>
    /// </remarks>
    public required float Size
    {
        get;
        init => field = Math.Max(float.Epsilon, value);
    }

    /// <summary>
    /// The style of this font
    /// </summary>
    public FontStyle Style { get; init; }

    /// <summary>
    /// The alignment of this font
    /// </summary>
    public StringAlignment Alignment { get; init; }

    /// <summary>
    /// Initializes new font information
    /// </summary>
    public FontInfo() { }

    /// <summary>
    /// Initializes new font information
    /// </summary>
    /// <param name="fontFamily">The font family name to use</param>
    /// <param name="size">The size to use</param>
    /// <param name="style">The style to use</param>
    /// <param name="alignment">The alignment to use</param>
    [SetsRequiredMembers]
    public FontInfo(string fontFamily, float size, FontStyle style = FontStyle.Regular, StringAlignment alignment = StringAlignment.Near)
    {
        FontFamily = fontFamily;
        Size = size;
        Style = style;
        Alignment = alignment;
    }
}