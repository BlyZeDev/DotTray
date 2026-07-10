namespace DotTray.Popup.Default;

using DotTray.Internal;
using DotTray.Internal.Native;
using DotTray.Internal.Win32;
using DotTray.Popup.Default.Common;
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
    /// <param name="color">The color to use</param>
    public void Fill(TrayColor color) => FillRect(Bounds, color);

    /// <summary>
    /// Fills the whole <paramref name="rect"/> with <paramref name="color"/>
    /// </summary>
    /// <param name="rect">The rectangle to fill</param>
    /// <param name="color">The color to use</param>
    public void FillRect(RectangleF rect, TrayColor color)
    {
        PInvoke.GdipCreateSolidFill(color.ToGdip(), out var hBrush);

        PInvoke.GdipSetSmoothingMode(_gdip, PInvoke.SmoothingModeHighSpeed);
        PInvoke.GdipFillRectangle(_gdip, hBrush, rect.X, rect.Y, rect.Width, rect.Height);

        PInvoke.GdipDeleteBrush(hBrush);
    }

    /// <summary>
    /// Write <paramref name="text"/> to the whole <see cref="Bounds"/>
    /// </summary>
    /// <param name="text">The text to write</param>
    /// <param name="font">The font to use</param>
    /// <param name="color">The color to use</param>
    public void Write(string text, FontInfo font, TrayColor color)
    {
        PInvoke.GdipCreateFontFamilyFromName(font.FontFamilyName, nint.Zero, out var hFamily);
        PInvoke.GdipCreateFont(hFamily, font.Size * Scale, 0, PInvoke.UnitPixel, out var hFont);

        PInvoke.GdipCreateStringFormat(0, 0, out var hFormat);
        PInvoke.GdipSetStringFormatFlags(hFormat, PInvoke.StringFormatFlagsNoWrap);
        PInvoke.GdipSetStringFormatAlign(hFormat, PInvoke.StringAlignmentNear);
        PInvoke.GdipSetStringFormatLineAlign(hFormat, PInvoke.StringAlignmentCenter);

        PInvoke.GdipCreateSolidFill(color.ToGdip(), out var hBrush);

        var layoutRect = Bounds.ToRECTF();
        text = SanitizeText(text);

        PInvoke.GdipSetTextRenderingHint(_gdip, PInvoke.TextRenderingHintAntiAliasGridFit);
        PInvoke.GdipDrawString(_gdip, text, text.Length, hFont, ref layoutRect, hFormat, hBrush);

        PInvoke.GdipDeleteBrush(hBrush);
        PInvoke.GdipDeleteStringFormat(hFormat);
        PInvoke.GdipDeleteFont(hFont);
        PInvoke.GdipDeleteFontFamily(hFamily);
    }

    private RectangleF Offset(RectangleF local) => new RectangleF(Bounds.X + local.X, Bounds.Y + local.Y, local.Width, local.Height);

    void IDisposable.Dispose()
    {

    }

    private static string SanitizeText(string text) => text.Replace("\uFE0F", "");
}