namespace DotTray;

/// <summary>
/// Represents the information of a PopupMenu
/// </summary>
public readonly record struct PopupMenuInfo
{
    /// <summary>
    /// Represents the GDI+ graphics handle used
    /// </summary>
    public readonly nint GdiGraphicsHandle { get; }

    /// <summary>
    /// Represents the GDI+ font family handle used
    /// </summary>
    public readonly nint FontFamilyHandle { get; }

    /// <summary>
    /// Represents the GDI+ font format handle used
    /// </summary>
    public readonly nint FontFormatHandle { get; }

    /// <summary>
    /// The scale factor calculated from the system DPI and base DPI of 96
    /// </summary>
    public readonly float Scale { get; }

    /// <summary>
    /// The absolute font size from the calling <see cref="NotifyIcon"/> instance
    /// </summary>
    public readonly float AbsoluteFontSize { get; }

    internal PopupMenuInfo(nint gdiGraphicsHandle, nint fontFamilyHandle, nint fontFormatHandle, float scale, float absoluteFontSize)
    {
        GdiGraphicsHandle = gdiGraphicsHandle;
        FontFamilyHandle = fontFamilyHandle;
        FontFormatHandle = fontFormatHandle;
        Scale = scale;
        AbsoluteFontSize = absoluteFontSize;
    }
}