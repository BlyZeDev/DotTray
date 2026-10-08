namespace DotTray.Direct.Internal;

using DotTray.Direct.Internal.Models;
using System.ComponentModel;
using System.Runtime.InteropServices;

internal static class PopupWindowHelper
{
    private const string ClassName = $"{nameof(DotTray)}{nameof(Direct)}PopupMenuWindow";

    private static readonly Lock _lock = new Lock();
    private static readonly PInvoke.WndProc _wndProc = WndProc;

    private static nint currentHInstance;
    private static nint currentHClassName;

    public static void EnsureRegistered(out nint hInstance, out nint hClassName)
    {
        lock (_lock)
        {
            if (currentHInstance == nint.Zero) currentHInstance = NativeLibrary.GetMainProgramHandle();

            if (currentHClassName == nint.Zero)
            {
                var name = Marshal.StringToHGlobalUni(ClassName);
                var wndClassEx = new WNDCLASSEX
                {
                    cbSize = (uint)Marshal.SizeOf<WNDCLASSEX>(),
                    lpfnWndProc = Marshal.GetFunctionPointerForDelegate(_wndProc),
                    hInstance = currentHInstance,
                    lpszClassName = name,
                    hbrBackground = nint.Zero,
                    hCursor = PInvoke.LoadCursor(nint.Zero, PInvoke.IDC_ARROW)
                };

                if (PInvoke.RegisterClassEx(ref wndClassEx) == 0)
                {
                    var error = Marshal.GetLastPInvokeError();
                    Marshal.FreeHGlobal(name);
                    throw new Win32Exception(error, "Registering the popup window class failed");
                }

                currentHClassName = name;
            }

            hInstance = currentHInstance;
            hClassName = currentHClassName;
        }
    }

    private static nint WndProc(nint hWnd, uint msg, nint wParam, nint lParam) => PInvoke.DefWindowProc(hWnd, msg, wParam, lParam);
}