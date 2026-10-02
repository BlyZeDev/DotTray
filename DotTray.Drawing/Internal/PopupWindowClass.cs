namespace DotTray.Drawing.Internal;

using DotTray.Internal.Native;
using DotTray.Internal.Win32;
using System.ComponentModel;
using System.Runtime.InteropServices;

internal static class PopupWindowClass
{
    private const string ClassName = $"{nameof(DotTray)}PopupMenuWindow";

    private static readonly Lock _lock = new Lock();
    private static readonly PInvoke.WndProc _wndProc = WndProc;

    private static nint currentHClassName;

    public static void EnsureRegistered(out nint hClassName)
    {
        lock (_lock)
        {
            if (currentHClassName == nint.Zero)
            {
                var instance = NativeLibrary.GetMainProgramHandle();

                var name = Marshal.StringToHGlobalUni(ClassName);
                var wndClass = new WNDCLASS
                {
                    lpfnWndProc = Marshal.GetFunctionPointerForDelegate(_wndProc),
                    hInstance = instance,
                    lpszClassName = name,
                    hbrBackground = nint.Zero,
                    hCursor = PInvoke.LoadCursor(nint.Zero, PInvoke.IDC_ARROW)
                };

                if (PInvoke.RegisterClass(ref wndClass) == 0)
                {
                    var error = Marshal.GetLastPInvokeError();
                    Marshal.FreeHGlobal(name);
                    throw new Win32Exception(error, "Registering the popup window class failed");
                }

                currentHClassName = name;
            }

            hClassName = currentHClassName;
        }
    }

    private static nint WndProc(nint hWnd, uint msg, nint wParam, nint lParam) => PInvoke.DefWindowProc(hWnd, msg, wParam, lParam);
}