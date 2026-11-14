namespace DotTray.Direct.Typography;

/// <summary>
/// Identifies values of <see cref="FontMetrics"/>
/// </summary>
[Flags]
public enum FontMetricValues
{
    /// <summary>No values</summary>
    None = 0,
    /// <summary><see cref="FontMetrics.Ascent"/></summary>
    Ascent = 1 << 0,
    /// <summary><see cref="FontMetrics.Descent"/></summary>
    Descent = 1 << 1,
    /// <summary><see cref="FontMetrics.LineGap"/></summary>
    LineGap = 1 << 2,
    /// <summary><see cref="FontMetrics.CapHeight"/></summary>
    CapHeight = 1 << 3,
    /// <summary><see cref="FontMetrics.XHeight"/></summary>
    XHeight = 1 << 4,
    /// <summary><see cref="FontMetrics.StrokeThickness"/></summary>
    StrokeThickness = 1 << 5,
    /// <summary><see cref="FontMetrics.UnderlinePosition"/></summary>
    UnderlinePosition = 1 << 6,
    /// <summary><see cref="FontMetrics.StrikethroughPosition"/></summary>
    StrikethroughPosition = 1 << 7
}