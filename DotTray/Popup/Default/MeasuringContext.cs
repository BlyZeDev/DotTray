namespace DotTray.Popup.Default;

using DotTray.Internal.Native;
using DotTray.Internal.Win32;
using DotTray.Popup.Default.Common;
using System;
using System.ComponentModel;
using System.Drawing;

/// <summary>
/// Includes data for measuring <see cref="MenuItemBase"/> instances
/// </summary>
public sealed class MeasuringContext : IDisposable
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
    /// The DPI scale factor of the monitor the menu is being shown on (1.0 = 96 DPI)
    /// </summary>
    /// <remarks>
    /// Multiply any DIP-based sizes by this to get actual pixels
    /// </remarks>
    public float Scale { get; }

    internal MeasuringContext(nint gdip, float scale)
    {
        _gdip = gdip;
        Scale = scale;
    }

    /// <summary>
    /// Measures the size, in pixels, required to render <paramref name="text"/> with <paramref name="font"/>
    /// </summary>
    /// <param name="text">The text to measure</param>
    /// <param name="font">The font to measure</param>
    /// <returns><see cref="SizeF"/></returns>
    public SizeF MeasureText(string text, FontInfo font)
    {
        PInvoke.GdipCreateFontFamilyFromName(font.FontFamilyName, nint.Zero, out var hFamily);
        PInvoke.GdipCreateFont(hFamily, font.Size * Scale, 0, PInvoke.UnitPixel, out var hFont);

        PInvoke.GdipCreateStringFormat(0, 0, out var hFormat);
        PInvoke.GdipSetStringFormatFlags(hFormat, PInvoke.StringFormatFlagsNoWrap);
        PInvoke.GdipSetStringFormatAlign(hFormat, PInvoke.StringAlignmentNear);
        PInvoke.GdipSetStringFormatLineAlign(hFormat, PInvoke.StringAlignmentCenter);

        var layoutRect = new RECTF
        {
            X = 0,
            Y = 0,
            Width = float.MaxValue,
            Height = float.MaxValue
        };

        text = SanitizeText(text);
        PInvoke.GdipMeasureString(_gdip, text, text.Length, hFont, ref layoutRect, hFormat, out var measured, out _, out _);

        PInvoke.GdipDeleteStringFormat(hFormat);
        PInvoke.GdipDeleteFont(hFont);
        PInvoke.GdipDeleteFontFamily(hFamily);

        return new SizeF(measured.Width, measured.Height);
    }

    void IDisposable.Dispose()
    {

    }

    private static string SanitizeText(string text) => text.Replace("\uFE0F", "");
}