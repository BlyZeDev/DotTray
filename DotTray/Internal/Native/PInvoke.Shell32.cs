namespace DotTray.Internal.Native;

using DotTray.Internal.Win32;
using System.Runtime.InteropServices;

internal static partial class PInvoke
{
    [DllImport(Shell32, CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool Shell_NotifyIcon(uint message, ref NOTIFYICONDATA data);
}