namespace DotTray.Direct.Coloring;

using DotTray.Direct.Internal;

/// <summary>
/// Describes how a gradient behaves outside of its <c>0..1</c> range
/// </summary>
public enum GradientExtendMode : uint
{
    /// <summary>Uses the first/last stop color</summary>
    Clamp = PInvoke.D2D1_EXTEND_MODE_CLAMP,
    /// <summary>Repeats the gradient</summary>
    Wrap = PInvoke.D2D1_EXTEND_MODE_WRAP,
    /// <summary>Repeats the gradient mirrored</summary>
    Mirror = PInvoke.D2D1_EXTEND_MODE_MIRROR
}