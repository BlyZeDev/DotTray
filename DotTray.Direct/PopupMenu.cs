namespace DotTray.Direct;

using DotTray.Direct.Internal;
using DotTray.Direct.Internal.Models;
using DotTray.Direct.Internal.Rendering;
using System.Runtime.InteropServices;

/// <summary>
/// Represents a Direct Popup Window
/// </summary>
public class PopupMenu
{
    private readonly nint _hWnd;
    private readonly PInvoke.WndProc _wndProc;
    private readonly DirectRenderer _renderer;

    public PopupMenu(nint ownerHWnd)
    {
        PInvoke.SetThreadDpiAwarenessContext(PInvoke.DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2);

        PopupWindowHelper.EnsureRegistered(out var hInstance, out var hClassName);

        _hWnd = PInvoke.CreateWindowEx(
            PInvoke.WS_EX_NOACTIVATE | PInvoke.WS_EX_TOOLWINDOW | PInvoke.WS_EX_TOPMOST | PInvoke.WS_EX_NOREDIRECTIONBITMAP,
            hClassName, nint.Zero,
            PInvoke.WS_CLIPCHILDREN | PInvoke.WS_CLIPSIBLINGS | PInvoke.WS_POPUP,
            100, 100, 500, 500,
            ownerHWnd, nint.Zero, hInstance, nint.Zero);
        PInvoke.ShowWindow(_hWnd, PInvoke.SW_SHOWNOACTIVATE);

        var dpi = PInvoke.GetDpiForWindow(_hWnd);
        _renderer = new DirectRenderer(_hWnd, 500, 500, dpi);

        _wndProc = new PInvoke.WndProc(WndProcFunc);
        PInvoke.SetWindowLongPtr(_hWnd, PInvoke.GWLP_WNDPROC, Marshal.GetFunctionPointerForDelegate(_wndProc));

        using (var draw = _renderer.BeginDraw())
        {
            draw.Context.Clear(new ColorF { A = 0, R = 0, G = 0, B = 0 });
            draw.Brush.SetColor(new ColorF { A = 1, R = 1, G = 1, B = 1 });
            draw.Context.FillRoundedRectangle(new RoundedRectF
            {
                Rect = new RectF
                {
                    Top = 0,
                    Left = 0,
                    Bottom = _renderer.SizeDips.Width,
                    Right = _renderer.SizeDips.Height
                },
                RadiusX = 16f,
                RadiusY = 16f
            }, draw.Brush);
        }
    }

    protected virtual nint WndProcFunc(nint hWnd, uint msg, nint wParam, nint lParam)
    {
        switch (msg)
        {
            
        }

        return PInvoke.DefWindowProc(hWnd, msg, wParam, lParam);
    }
}