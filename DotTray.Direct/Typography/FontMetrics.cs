namespace DotTray.Direct.Typography;

using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Describes the vertical metrics of a font at a specific size, all values are in DIPs
/// </summary>
/// <remarks>
/// Values the font did not provide are <see langword="null"/>.<br/>
/// Use the <c>...OrEstimate</c> members when an approximation is good enough, and <see cref="Unavailable"/> to see what was estimated.<br/>
/// <see cref="UnderlinePosition"/> and <see cref="StrikethroughPosition"/> are offsets from the baseline (positive is above), everything else is measured from the top of the line
/// </remarks>
public readonly record struct FontMetrics
{
    private const float CapHeightEstimateRatio = 0.7f;
    private const float XHeightEstimateRatio = 0.5f;
    private const float StrokeEstimateRatio = 0.07f;
    private const float UnderlineEstimateRatio = 0.1f;

    /// <summary>
    /// The default <see cref="FontMetrics"/> instance, with <see cref="Size"/> zero and every optional value unavailable
    /// </summary>
    public static readonly FontMetrics Empty = default;

    /// <summary>The font size (the em height)</summary>
    public required readonly float Size { get; init; }

    /// <summary>The height of a single line of text, including the line gap</summary>
    public required readonly float LineHeight { get; init; }

    /// <summary>The distance from the top of the line to the baseline</summary>
    public required readonly float Baseline { get; init; }

    /// <summary>The font's ascent above the baseline, or <see langword="null"/> if unavailable</summary>
    public required readonly float? Ascent { get; init; }

    /// <summary>The font's descent below the baseline, or <see langword="null"/> if unavailable</summary>
    public required readonly float? Descent { get; init; }

    /// <summary>The extra spacing the font recommends between lines, or <see langword="null"/> if unavailable</summary>
    public required readonly float? LineGap { get; init; }

    /// <summary>The height of capital letters above the baseline, or <see langword="null"/> if unavailable</summary>
    public required readonly float? CapHeight { get; init; }

    /// <summary>The height of lowercase letters without ascenders above the baseline, or <see langword="null"/> if unavailable</summary>
    public required readonly float? XHeight { get; init; }

    /// <summary>The font's natural stroke weight (its underline thickness), or <see langword="null"/> if unavailable</summary>
    public required readonly float? StrokeThickness { get; init; }

    /// <summary>The offset of the underline from the baseline, or <see langword="null"/> if unavailable</summary>
    public required readonly float? UnderlinePosition { get; init; }

    /// <summary>The offset of the strikethrough from the baseline, or <see langword="null"/> if unavailable</summary>
    public required readonly float? StrikethroughPosition { get; init; }

    /// <summary><see cref="Ascent"/>, or the distance above the baseline derived from the line if unavailable</summary>
    public readonly float AscentOrEstimate => Ascent ?? Baseline;

    /// <summary><see cref="Descent"/>, or the distance below the baseline derived from the line if unavailable</summary>
    public readonly float DescentOrEstimate => Descent ?? Math.Max(0f, LineHeight - Baseline - (LineGap ?? 0f));

    /// <summary><see cref="LineGap"/>, or zero if unavailable</summary>
    public readonly float LineGapOrEstimate => LineGap ?? 0f;

    /// <summary><see cref="CapHeight"/>, or a typical proportion of <see cref="Size"/> if unavailable</summary>
    public readonly float CapHeightOrEstimate => CapHeight ?? Size * CapHeightEstimateRatio;

    /// <summary><see cref="XHeight"/>, or a typical proportion of <see cref="Size"/> if unavailable</summary>
    public readonly float XHeightOrEstimate => XHeight ?? Size * XHeightEstimateRatio;

    /// <summary><see cref="StrokeThickness"/>, or a typical proportion of <see cref="Size"/> if unavailable</summary>
    public readonly float StrokeThicknessOrEstimate => StrokeThickness ?? Size * StrokeEstimateRatio;

    /// <summary><see cref="UnderlinePosition"/>, or a typical offset below the baseline if unavailable</summary>
    public readonly float UnderlinePositionOrEstimate => UnderlinePosition ?? -Size * UnderlineEstimateRatio;

    /// <summary><see cref="StrikethroughPosition"/>, or half of <see cref="XHeightOrEstimate"/> if unavailable</summary>
    public readonly float StrikethroughPositionOrEstimate => StrikethroughPosition ?? XHeightOrEstimate / 2f;

    /// <summary>The distance from the top of the line to the vertical center of capital letters</summary>
    public readonly float CapCenter => Baseline - CapHeightOrEstimate / 2f;

    /// <summary>The distance from the top of the line to the vertical center of lowercase letters</summary>
    public readonly float XCenter => Baseline - XHeightOrEstimate / 2f;

    /// <summary>The values the font did not provide, so the <c>...OrEstimate</c> members return estimates for them</summary>
    public readonly FontMetricValues Unavailable =>
        (Ascent is null ? FontMetricValues.Ascent : 0) |
        (Descent is null ? FontMetricValues.Descent : 0) |
        (LineGap is null ? FontMetricValues.LineGap : 0) |
        (CapHeight is null ? FontMetricValues.CapHeight : 0) |
        (XHeight is null ? FontMetricValues.XHeight : 0) |
        (StrokeThickness is null ? FontMetricValues.StrokeThickness : 0) |
        (UnderlinePosition is null ? FontMetricValues.UnderlinePosition : 0) |
        (StrikethroughPosition is null ? FontMetricValues.StrikethroughPosition : 0);

    /// <summary><see langword="true"/> if the font provided every value, otherwise <see langword="false"/></summary>
    public readonly bool IsComplete => Unavailable is FontMetricValues.None;

    /// <summary>
    /// Initializes a new <see cref="FontMetrics"/> instance, pass <see langword="null"/> for values that are not available
    /// </summary>
    [SetsRequiredMembers]
    public FontMetrics(
        float size,
        float lineHeight,
        float baseline,
        float? ascent,
        float? descent,
        float? lineGap,
        float? capHeight,
        float? xHeight,
        float? strokeThickness,
        float? underlinePosition,
        float? strikethroughPosition)
    {
        Size = size;
        LineHeight = lineHeight;
        Baseline = baseline;
        Ascent = ascent;
        Descent = descent;
        LineGap = lineGap;
        CapHeight = capHeight;
        XHeight = xHeight;
        StrokeThickness = strokeThickness;
        UnderlinePosition = underlinePosition;
        StrikethroughPosition = strikethroughPosition;
    }

    /// <summary>
    /// The distance from the top of <paramref name="boundsHeight"/> to the top of a vertically centered line
    /// </summary>
    /// <param name="boundsHeight">The height of the bounds</param>
    /// <returns><see cref="float"/></returns>
    public readonly float LineTop(float boundsHeight) => (boundsHeight - LineHeight) / 2f;

    /// <summary>
    /// The distance from the top of <paramref name="boundsHeight"/> to the baseline of a vertically centered line
    /// </summary>
    /// <param name="boundsHeight">The height of the bounds</param>
    /// <returns><see cref="float"/></returns>
    public readonly float BaselineIn(float boundsHeight) => LineTop(boundsHeight) + Baseline;

    /// <summary>
    /// The distance from the top of <paramref name="boundsHeight"/> to the center of capital letters of a vertically centered line
    /// </summary>
    /// <param name="boundsHeight">The height of the bounds</param>
    /// <returns><see cref="float"/></returns>
    public readonly float CapCenterIn(float boundsHeight) => LineTop(boundsHeight) + CapCenter;

    /// <summary>
    /// The distance from the top of <paramref name="boundsHeight"/> to the center of lowercase letters of a vertically centered line
    /// </summary>
    /// <param name="boundsHeight">The height of the bounds</param>
    /// <returns><see cref="float"/></returns>
    public readonly float XCenterIn(float boundsHeight) => LineTop(boundsHeight) + XCenter;
}