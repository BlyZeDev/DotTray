namespace DotTray.Direct.Typography;

using DotTray.Direct.Internal;

/// <summary>
/// Represents the font style
/// </summary>
public enum FontStyle
{
    /// <summary>Normal</summary>
    Normal = PInvoke.DWRITE_FONT_STYLE_NORMAL,

    /// <summary>Oblique</summary>
    Oblique = PInvoke.DWRITE_FONT_STYLE_OBLIQUE,

    /// <summary>Italic</summary>
    Italic = PInvoke.DWRITE_FONT_STYLE_ITALIC
}