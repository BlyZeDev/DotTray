namespace DotTray.Direct.Typography;

using DotTray.Direct.Internal;

/// <summary>
/// Represents the font stretch
/// </summary>
public enum FontStretch
{
    /// <summary>Not known</summary>
    Undefined = PInvoke.DWRITE_FONT_STRETCH_UNDEFINED,

    /// <summary>Ultra-condensed</summary>
    UltraCondensed = PInvoke.DWRITE_FONT_STRETCH_ULTRA_CONDENSED,

    /// <summary>Extra-condensed</summary>
    ExtraCondensed = PInvoke.DWRITE_FONT_STRETCH_EXTRA_CONDENSED,

    /// <summary>Condensed</summary>
    Condensed = PInvoke.DWRITE_FONT_STRETCH_CONDENSED,

    /// <summary>Semi-condensed</summary>
    SemiCondensed = PInvoke.DWRITE_FONT_STRETCH_SEMI_CONDENSED,

    /// <summary>Normal</summary>
    Normal = PInvoke.DWRITE_FONT_STRETCH_NORMAL,

    /// <summary>Medium</summary>
    Medium = PInvoke.DWRITE_FONT_STRETCH_MEDIUM,

    /// <summary>Semi-expanded</summary>
    SemiExpanded = PInvoke.DWRITE_FONT_STRETCH_SEMI_EXPANDED,

    /// <summary>Expanded</summary>
    Expanded = PInvoke.DWRITE_FONT_STRETCH_EXPANDED,

    /// <summary>Extra-expanded</summary>
    ExtraExpanded = PInvoke.DWRITE_FONT_STRETCH_EXTRA_EXPANDED,

    /// <summary>Ultra-expanded</summary>
    UltraExpanded = PInvoke.DWRITE_FONT_STRETCH_ULTRA_EXPANDED
}