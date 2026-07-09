namespace DotTray.Popup.Default;

using DotTray.Internal;
using DotTray.Internal.Native;
using DotTray.Popup.Default.Abstract;
using System;
using System.ComponentModel;
using System.Drawing;

/// <summary>
/// Includes data for drawing <see cref="MenuItemBase"/> instances
/// </summary>
public sealed class DrawingContext : IDisposable
{
    private readonly nint _gdip;

    /// <summary>
    /// The raw GDI+ graphics handle backing this context
    /// </summary>
    /// <remarks>
    /// Use this if you want native control over the drawing process.<br/>
    /// <b>Use with caution</b>
    /// </remarks>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public nint NativeGraphicsHandle => _gdip;

    /// <summary>
    /// The bounds, in window client coordinates, assigned to the item currently being drawn
    /// </summary>
    /// <remarks>
    /// This is set immediately before each item's <see cref="MenuItemBase.Draw(DrawingContext)"/> is called
    /// </remarks>
    public RectangleF Bounds { get; internal set; }

    /// <summary>
    /// The DPI scale factor of the monitor the menu is being shown on (1.0 = 96 DPI)
    /// </summary>
    /// <remarks>
    /// Multiply any DIP-based sizes by this to get actual pixels
    /// </remarks>
    public float Scale { get; }

    internal DrawingContext(nint gdip, float scale)
    {
        _gdip = gdip;
        Scale = scale;
    }

    /// <summary>
    /// Fills the whole <see cref="Bounds"/> with <paramref name="color"/>
    /// </summary>
    /// <param name="color">The color to fill</param>
    public void Fill(TrayColor color)
    {
        var target = Offset(Local());
        PInvoke.GdipCreateSolidFill(color.ToGdip(), out var hBrush);
        PInvoke.GdipFillRectangle(_gdip, hBrush, target.X, target.Y, target.Width, target.Height);
        PInvoke.GdipDeleteBrush(hBrush);
    }

    private RectangleF Local() => new RectangleF(0, 0, Bounds.Width, Bounds.Height);
    private RectangleF Offset(RectangleF local) => new RectangleF(Bounds.X + local.X, Bounds.Y + local.Y, local.Width, local.Height);


    void IDisposable.Dispose()
    {

    }
}