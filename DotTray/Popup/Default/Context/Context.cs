namespace DotTray.Popup.Default.Context;

using System;
using System.ComponentModel;

/// <summary>
/// Represents the context base
/// </summary>
public abstract class Context : IDisposable
{
    internal readonly nint _gdip;

    /// <summary>
    /// The raw GDI+ graphics handle backing this context
    /// </summary>
    /// <remarks>
    /// <b>Caution:</b> Use this if you want native control over the drawing process
    /// </remarks>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public nint NativeGraphicsHandle => _gdip;

    /// <summary>
    /// The DPI scale factor of the monitor the menu is being shown on (1.0 = 96 DPI)
    /// </summary>
    public float Scale { get; }

    internal Context(nint gdip, float scale)
    {
        _gdip = gdip;
        Scale = scale;
    }

    internal virtual void DisposeCore() { }

    void IDisposable.Dispose()
    {
        DisposeCore();
        GC.SuppressFinalize(this);
    }

    internal static string SanitizeText(string text) => text.Replace("\uFE0F", "");
}