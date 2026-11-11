namespace DotTray.Direct.Typography;

using System.Globalization;

/// <summary>
/// Describes how a text should be rendered
/// </summary>
public sealed record TextInfo
{
    /// <summary>The smallest accepted size in DIPs</summary>
    public const float MinSize = 1f;
    /// <summary>The largest accepted size in DIPs</summary>
    public const float MaxSize = 1000f;

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
    /// <remarks>
    /// This value is clamped between <c><see cref="MinSize"/></c> and <c><see cref="MaxSize"/></c><br/>
    /// <c><see cref="float.NaN"/></c> becomes <c><see cref="MinSize"/></c>
    /// </remarks>
    public required float Size
    {
        get;
        init => field = float.IsNaN(value) ? MinSize : Math.Clamp(value, MinSize, MaxSize);
    }

    /// <summary>
    /// The weight of the font
    /// </summary>
    public FontWeight Weight { get; init; } = FontWeight.Normal;

    /// <summary>
    /// The style of the font
    /// </summary>
    /// <remarks>
    /// Throws an <see cref="ArgumentOutOfRangeException"/> if the value is not a defined <see cref="FontStyle"/>
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public FontStyle Style
    {
        get;
        init => field = Enum.IsDefined(value) ? value : throw new ArgumentOutOfRangeException(nameof(value), "Invalid enum value");
    } = FontStyle.Normal;

    /// <summary>
    /// The stretching of the font
    /// </summary>
    /// <remarks>
    /// Throws an <see cref="ArgumentOutOfRangeException"/> if the value is not a defined <see cref="FontStretch"/>
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public FontStretch Stretch
    {
        get;
        init => field = Enum.IsDefined(value) ? value : throw new ArgumentOutOfRangeException(nameof(value), "Invalid enum value");
    } = FontStretch.Normal;

    /// <summary>
    /// The alignment of the text
    /// </summary>
    /// <remarks>
    /// Throws an <see cref="ArgumentOutOfRangeException"/> if the value is not a defined <see cref="TextAlign"/>
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public TextAlign Align
    {
        get;
        init => field = Enum.IsDefined(value) ? value : throw new ArgumentOutOfRangeException(nameof(value), "Invalid enum value");
    } = TextAlign.Left;

    /// <summary>
    /// The language used for text formatting and glyph selection
    /// </summary>
    public LanguageTag Language { get; init; } = new LanguageTag(CultureInfo.CurrentUICulture);
}