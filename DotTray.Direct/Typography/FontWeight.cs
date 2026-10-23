namespace DotTray.Direct.Typography;

using DotTray.Direct.Internal;
using System.Numerics;

/// <summary>
/// Represents the weight of a font
/// </summary>
public readonly record struct FontWeight : IMinMaxValue<FontWeight>
{
    /// <summary>Minimum font weight (1)</summary>
    public static FontWeight MinValue => new FontWeight(PInvoke.DWRITE_FONT_WEIGHT_MIN);
    /// <summary>Maximum font weight (999)</summary>
    public static FontWeight MaxValue => new FontWeight(PInvoke.DWRITE_FONT_WEIGHT_MAX);

    /// <summary>Thin</summary>
    public static readonly FontWeight Thin = new FontWeight(PInvoke.DWRITE_FONT_WEIGHT_THIN);

    /// <summary>Extra light</summary>
    public static readonly FontWeight ExtraLight = new FontWeight(PInvoke.DWRITE_FONT_WEIGHT_EXTRA_LIGHT);

    /// <summary>Ultra light</summary>
    public static readonly FontWeight UltraLight = new FontWeight(PInvoke.DWRITE_FONT_WEIGHT_ULTRA_LIGHT);

    /// <summary>Light</summary>
    public static readonly FontWeight Light = new FontWeight(PInvoke.DWRITE_FONT_WEIGHT_LIGHT);

    /// <summary>Semi light</summary>
    public static readonly FontWeight SemiLight = new FontWeight(PInvoke.DWRITE_FONT_WEIGHT_SEMI_LIGHT);

    /// <summary>Normal</summary>
    public static readonly FontWeight Normal = new FontWeight(PInvoke.DWRITE_FONT_WEIGHT_NORMAL);

    /// <summary>Regular</summary>
    public static readonly FontWeight Regular = new FontWeight(PInvoke.DWRITE_FONT_WEIGHT_REGULAR);

    /// <summary>Medium</summary>
    public static readonly FontWeight Medium = new FontWeight(PInvoke.DWRITE_FONT_WEIGHT_MEDIUM);

    /// <summary>Demi bold</summary>
    public static readonly FontWeight DemiBold = new FontWeight(PInvoke.DWRITE_FONT_WEIGHT_DEMI_BOLD);

    /// <summary>Semi bold</summary>
    public static readonly FontWeight SemiBold = new FontWeight(PInvoke.DWRITE_FONT_WEIGHT_SEMI_BOLD);

    /// <summary>Bold</summary>
    public static readonly FontWeight Bold = new FontWeight(PInvoke.DWRITE_FONT_WEIGHT_BOLD);

    /// <summary>Extra bold</summary>
    public static readonly FontWeight ExtraBold = new FontWeight(PInvoke.DWRITE_FONT_WEIGHT_EXTRA_BOLD);

    /// <summary>Ultra bold</summary>
    public static readonly FontWeight UltraBold = new FontWeight(PInvoke.DWRITE_FONT_WEIGHT_ULTRA_BOLD);

    /// <summary>Black</summary>
    public static readonly FontWeight Black = new FontWeight(PInvoke.DWRITE_FONT_WEIGHT_BLACK);

    /// <summary>Heavy</summary>
    public static readonly FontWeight Heavy = new FontWeight(PInvoke.DWRITE_FONT_WEIGHT_HEAVY);

    /// <summary>Extra black</summary>
    public static readonly FontWeight ExtraBlack = new FontWeight(PInvoke.DWRITE_FONT_WEIGHT_EXTRA_BLACK);

    /// <summary>Ultra black</summary>
    public static readonly FontWeight UltraBlack = new FontWeight(PInvoke.DWRITE_FONT_WEIGHT_ULTRA_BLACK);

    /// <summary>
    /// The value of this <see cref="FontWeight"/> instance
    /// </summary>
    public readonly int Value { get; }

    /// <summary>
    /// Initializes a new <see cref="FontWeight"/>
    /// </summary>
    /// <remarks>
    /// <paramref name="value"/> is clamped between <see cref="MinValue"/> and <see cref="MaxValue"/>
    /// </remarks>
    /// <param name="value">The value to use</param>
    public FontWeight(int value) => Value = Math.Clamp(value, PInvoke.DWRITE_FONT_WEIGHT_MIN, PInvoke.DWRITE_FONT_WEIGHT_MAX);

    /// <summary>
    /// Implicitly converts <see cref="FontWeight"/> to its <see cref="int"/> value
    /// </summary>
    /// <param name="weight">The font weight</param>
    public static implicit operator int(FontWeight weight) => weight.Value;
}