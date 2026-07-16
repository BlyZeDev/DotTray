namespace DotTray.Popup.Default.Coloring;

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
    /// Creates a native GDI+ handle for coloring purpose
    /// </summary>
    /// <remarks>
    /// Implementing this requires working with native GDI+ handles.<br/>
    /// <b>Use with caution</b>
    /// </remarks>
    /// <param name="bounds">The bounds to color</param>
    /// <returns><see cref="SafeHandle"/></returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal protected SafeHandle CreateNativeHandle(RectangleF bounds);
}