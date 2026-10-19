namespace DotTray.Direct.Context;

/// <summary>
/// Represents the context base
/// </summary>
/// <remarks>
/// All sizes and coordinates handled by contexts are in DIPs (1 DIP = 1 pixel at 100% display scaling)
/// </remarks>
public abstract class Context
{
    /// <summary>
    /// The DPI scale factor of the monitor the menu is being shown on (1.0 = 96 DPI)
    /// </summary>
    /// <remarks>
    /// The scale is already accounted for since everything is measured and drawn in DIPs, so this value is mostly informational
    /// </remarks>
    public float Scale { get; }

    internal Context(float scale)
    {
        Scale = scale;
    }
}