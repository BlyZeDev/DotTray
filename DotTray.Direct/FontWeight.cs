namespace DotTray.Direct;

using System.Numerics;

/// <summary>
/// Represents the weight of a font.
/// </summary>
public readonly record struct FontWeight : IMinMaxValue<FontWeight>
{
    private const int Min = 1;
    private const int Max = 999;

    /// <summary>Minimum font weight (1).</summary>
    public static FontWeight MinValue => new FontWeight(Min);
    /// <summary>Maximum font weight (999).</summary>
    public static FontWeight MaxValue => new FontWeight(Max);

    /// <summary>Thin</summary>
    public static readonly FontWeight Thin = new FontWeight(100);

    /// <summary>Extra light</summary>
    public static readonly FontWeight ExtraLight = new FontWeight(200);

    /// <summary>Ultra light</summary>
    public static readonly FontWeight UltraLight = new FontWeight(200);

    /// <summary>Light</summary>
    public static readonly FontWeight Light = new FontWeight(300);

    /// <summary>Semi light</summary>
    public static readonly FontWeight SemiLight = new FontWeight(350);

    /// <summary>Normal</summary>
    public static readonly FontWeight Normal = new FontWeight(400);

    /// <summary>Regular</summary>
    public static readonly FontWeight Regular = new FontWeight(400);

    /// <summary>Medium</summary>
    public static readonly FontWeight Medium = new FontWeight(500);

    /// <summary>Demi bold</summary>
    public static readonly FontWeight DemiBold = new FontWeight(600);

    /// <summary>Semi bold</summary>
    public static readonly FontWeight SemiBold = new FontWeight(600);

    /// <summary>Bold</summary>
    public static readonly FontWeight Bold = new FontWeight(700);

    /// <summary>Extra bold</summary>
    public static readonly FontWeight ExtraBold = new FontWeight(800);

    /// <summary>Ultra bold</summary>
    public static readonly FontWeight UltraBold = new FontWeight(800);

    /// <summary>Black</summary>
    public static readonly FontWeight Black = new FontWeight(900);

    /// <summary>Heavy</summary>
    public static readonly FontWeight Heavy = new FontWeight(900);

    /// <summary>Extra black</summary>
    public static readonly FontWeight ExtraBlack = new FontWeight(950);

    /// <summary>Ultra black</summary>
    public static readonly FontWeight UltraBlack = new FontWeight(950);

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
    public FontWeight(int value) => Value = Math.Clamp(value, Min, Max);

    /// <summary>
    /// Implicitly converts <see cref="FontWeight"/> to its <see cref="int"/> value
    /// </summary>
    /// <param name="weight">The font weight</param>
    public static implicit operator int(FontWeight weight) => weight.Value;
}