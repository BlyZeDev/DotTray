namespace DotTray.Direct.Typography;

using System.Globalization;

/// <summary>
/// Describes how a text should be rendered
/// </summary>
public sealed record TextInfo
{
    /// <summary>
    /// Segoe UI Emoji, 16 DIPs
    /// </summary>
    public static TextInfo Default => new TextInfo
    {
        Font = SystemFont.Default,
        Size = 16f
    };

    /// <summary>
    /// The font used, eg. <see cref="SystemFont.Default"/>
    /// </summary>
    public required Font Font { get; init; }

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

    /// <summary>
    /// The language used for text formatting and glyph selection
    /// </summary>
    public LanguageTag Language { get; init; } = new LanguageTag(CultureInfo.CurrentUICulture);
}