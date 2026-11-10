namespace DotTray.Direct.Imaging;

using DotTray.Direct.Internal;

/// <summary>
/// Represents the interpolation mode used for image scaling or drawing operations
/// </summary>
public enum InterpolationMode : uint
{
    /// <summary>Nearest neighbor interpolation</summary>
    NearestNeighbor = PInvoke.D2D1_INTERPOLATION_MODE_NEAREST_NEIGHBOR,

    /// <summary>Linear interpolation</summary>
    Linear = PInvoke.D2D1_INTERPOLATION_MODE_LINEAR,

    /// <summary>Cubic interpolation</summary>
    Cubic = PInvoke.D2D1_INTERPOLATION_MODE_CUBIC,

    /// <summary>Multi-sample linear interpolation</summary>
    MultiSampleLinear = PInvoke.D2D1_INTERPOLATION_MODE_MULTI_SAMPLE_LINEAR,

    /// <summary>Anisotropic interpolation</summary>
    Anisotropic = PInvoke.D2D1_INTERPOLATION_MODE_ANISOTROPIC,

    /// <summary>High-quality cubic interpolation</summary>
    HighQualityCubic = PInvoke.D2D1_INTERPOLATION_MODE_HIGH_QUALITY_CUBIC
}