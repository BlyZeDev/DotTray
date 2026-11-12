namespace DotTray.Direct.Typography;

using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Describes the vertical metrics of a font at a specific size, all values are in DIPs
/// </summary>
public readonly record struct FontMetrics
{
    /// <summary>
    /// The default <see cref="FontMetrics"/> instance
    /// </summary>
    public static readonly FontMetrics Empty = default;

    /// <summary>
    /// The height of a single line of text
    /// </summary>
    public required readonly float LineHeight { get; init; }

    /// <summary>
    /// The distance from the top of the line to the baseline
    /// </summary>
    public required readonly float Baseline { get; init; }

    /// <summary>
    /// The height of capital letters above the baseline
    /// </summary>
    public required readonly float CapHeight { get; init; }

    /// <summary>
    /// The distance from the top of the line to the vertical center of capital letters
    /// </summary>
    public readonly float CapCenter => Baseline - CapHeight / 2f;

    /// <summary>
    /// Initializes a new <see cref="FontMetrics"/> instance
    /// </summary>
    /// <param name="lineHeight">The height of a single line of text</param>
    /// <param name="baseline">The distance from the top of the line to the baseline</param>
    /// <param name="capHeight">The height of capital letters above the baseline</param>
    [SetsRequiredMembers]
    public FontMetrics(float lineHeight, float baseline, float capHeight)
    {
        LineHeight = lineHeight;
        Baseline = baseline;
        CapHeight = capHeight;
    }
}