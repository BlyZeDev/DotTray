namespace DotTray.Direct.Context;

using DotTray.Direct.Internal.Rendering;
using DotTray.Direct.Primitives;
using DotTray.Direct.Typography;
using System.ComponentModel;

/// <summary>
/// Represents the context base
/// </summary>
/// <remarks>
/// All sizes and coordinates handled by contexts are in DIPs (1 DIP = 1 pixel at 100% display scaling)
/// </remarks>
public abstract class Context
{
    private protected readonly DirectSurface _surface;

    /// <summary>
    /// The native <c>ID2D1DeviceContext*</c> the window is drawn on
    /// </summary>
    /// <remarks>
    /// The pointer is borrowed: don't release it and don't use it after the draw call
    /// </remarks>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public nint NativeContext => _surface.GetContextPointer();

    /// <summary>
    /// The DPI scale factor of the monitor the menu is being shown on (1.0 = 96 DPI)
    /// </summary>
    /// <remarks>
    /// The scale is already accounted for since everything is measured and drawn in DIPs, so this value is mostly informational
    /// </remarks>
    public float Scale => _surface.Scale;

    internal Context(DirectSurface surface) => _surface = surface;

    /// <summary>
    /// Measures the size, in DIPs, required to render <paramref name="text"/> with <paramref name="textInfo"/>
    /// </summary>
    /// <param name="text">The text to measure</param>
    /// <param name="textInfo">The text information to measure the text for</param>
    /// <returns><see cref="DimF"/></returns>
    public DimF MeasureText(string text, TextInfo textInfo) => _surface.MeasureText(text, textInfo);
}