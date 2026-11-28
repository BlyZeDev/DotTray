namespace DotTray.Direct.Internal.Popup;

using DotTray.Direct.Internal.Models;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

internal static unsafe class PopupWindowClass
{
    private const string PopupClassName = $"{nameof(DotTray)}{nameof(Direct)}PopupMenuWindow";
    private const string DispatcherClassName = $"{nameof(DotTray)}{nameof(Direct)}PopupDispatcherWindow";

    private static readonly Lock _lock = new Lock();

    private static nint hInstance;
    private static nint popupClass;
    private static nint dispatcherClass;

    public static nint ArrowCursor { get; }

    static PopupWindowClass() => ArrowCursor = PInvoke.LoadCursor(nint.Zero, PInvoke.IDC_ARROW);

    public static void EnsureRegistered(out nint instance, out nint popup, out nint dispatcher)
    {
        lock (_lock)
        {
            if (hInstance == nint.Zero) hInstance = NativeLibrary.GetMainProgramHandle();
            if (popupClass == nint.Zero) popupClass = Register(PopupClassName, (nint)(delegate* unmanaged[Stdcall]<nint, uint, nint, nint, nint>)&PopupWndProc, PInvoke.CS_DROPSHADOW, true);
            if (dispatcherClass == nint.Zero) dispatcherClass = Register(DispatcherClassName, (nint)(delegate* unmanaged[Stdcall]<nint, uint, nint, nint, nint>)&DispatcherWndProc, 0, false);

            instance = hInstance;
            popup = popupClass;
            dispatcher = dispatcherClass;
        }
    }

    public static nint CreatePopupWindow(nint state, uint exStyle, int x, int y, nint ownerHWnd)
    {
        EnsureRegistered(out var instance, out var popup, out _);

        var hWnd = PInvoke.CreateWindowEx(
            exStyle,
            popup, nint.Zero,
            PInvoke.WS_CLIPCHILDREN | PInvoke.WS_CLIPSIBLINGS | PInvoke.WS_POPUP,
            x, y, 1, 1,
            ownerHWnd, nint.Zero, instance, state);

        return hWnd == nint.Zero ? throw new Win32Exception(Marshal.GetLastPInvokeError(), "Creating the popup window failed") : hWnd;
    }

    public static nint CreateDispatcherWindow(nint state)
    {
        EnsureRegistered(out var instance, out _, out var dispatcher);

        var hWnd = PInvoke.CreateWindowEx(0, dispatcher, nint.Zero, 0, 0, 0, 0, 0, PInvoke.HWND_MESSAGE, nint.Zero, instance, state);

        return hWnd == nint.Zero ? throw new Win32Exception(Marshal.GetLastPInvokeError(), "Creating the popup dispatcher window failed") : hWnd;
    }

    private static nint Register(string className, nint wndProc, uint style, bool useArrowCursor)
    {
        var name = Marshal.StringToHGlobalUni(className);
        var wndClassEx = new WNDCLASSEX
        {
            cbSize = (uint)Unsafe.SizeOf<WNDCLASSEX>(),
            lpfnWndProc = wndProc,
            hInstance = hInstance,
            lpszClassName = name,
            hbrBackground = nint.Zero,
            hCursor = useArrowCursor ? ArrowCursor : nint.Zero,
            style = style
        };

        if (PInvoke.RegisterClassEx(ref wndClassEx) == 0)
        {
            var error = Marshal.GetLastPInvokeError();
            Marshal.FreeHGlobal(name);
            throw new Win32Exception(error, $"Registering the window class {className} failed");
        }

        return name;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static nint PopupWndProc(nint hWnd, uint msg, nint wParam, nint lParam)
    {
        if (msg == PInvoke.WM_NCCREATE)
        {
            PInvoke.SetWindowLongPtr(hWnd, PInvoke.GWLP_USERDATA, Marshal.ReadIntPtr(lParam));
            return PInvoke.DefWindowProc(hWnd, msg, wParam, lParam);
        }

        var state = PInvoke.GetWindowLongPtr(hWnd, PInvoke.GWLP_USERDATA);
        if (state == nint.Zero) return PInvoke.DefWindowProc(hWnd, msg, wParam, lParam);

        try
        {
            var window = (PopupWindow)GCHandle.FromIntPtr(state).Target!;
            return window.HandleMessage(hWnd, msg, wParam, lParam);
        }
        catch (Exception ex)
        {
            PopupThread.Fail(ex);
            return 0;
        }
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static nint DispatcherWndProc(nint hWnd, uint msg, nint wParam, nint lParam)
    {
        if (msg == PInvoke.WM_NCCREATE)
        {
            PInvoke.SetWindowLongPtr(hWnd, PInvoke.GWLP_USERDATA, Marshal.ReadIntPtr(lParam));
            return PInvoke.DefWindowProc(hWnd, msg, wParam, lParam);
        }

        var state = PInvoke.GetWindowLongPtr(hWnd, PInvoke.GWLP_USERDATA);
        if (state == nint.Zero) return PInvoke.DefWindowProc(hWnd, msg, wParam, lParam);

        try
        {
            var thread = (PopupThread)GCHandle.FromIntPtr(state).Target!;
            return thread.HandleDispatcherMessage(hWnd, msg, wParam, lParam);
        }
        catch (Exception ex)
        {
            PopupThread.Fail(ex);
            return 0;
        }
    }
}