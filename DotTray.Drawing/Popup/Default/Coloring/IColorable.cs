namespace DotTray.Popup.Default.Coloring;

using DotTray.Internal.Native;
using DotTray.Primitives;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

/// <summary>
/// Represents a contract that defines something colorable 
/// </summary>
public interface IColorable : IEquatable<IColorable>
{
    /// <summary>
    /// Creates a native GDI+ brush handle
    /// </summary>
    /// <remarks>
    /// Implementing this requires working with native GDI+ handles.<br/>
    /// <b>Use with caution</b>
    /// </remarks>
    /// <param name="bounds">The bounds to color</param>
    /// <returns><see cref="SafeHandle"/></returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    SafeHandle CreateGdipBrush(RectangleF bounds);

    /// <summary>
    /// Creates a native GDI+ pen handle
    /// </summary>
    /// <param name="bounds">The bounds to apply layout or gradient coordinates</param>
    /// <param name="width">The stroke widht of the pen in pixels</param>
    /// <returns><see cref="SafeHandle"/></returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    SafeHandle CreateGdipPen(RectangleF bounds, float width = 2f)
    {
        using (var hBrush = CreateGdipBrush(bounds))
        {
            PInvoke.GdipCreatePen2(hBrush.DangerousGetHandle(), width, PInvoke.UnitPixel, out var hPen);
            return new PenSafeHandle(hPen);
        }
    }

    /// <summary>
    /// Creates a native GDI+ pen handle
    /// </summary>
    /// <param name="width">The stroke widht of the pen in pixels</param>
    /// <returns><see cref="SafeHandle"/></returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    SafeHandle CreateGdipPen(float width = 2f) => CreateGdipPen(new RectangleF(0, 0, 1, 1), width);
}