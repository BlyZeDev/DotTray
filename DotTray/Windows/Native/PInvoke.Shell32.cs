namespace DotTray.Windows.Native;

using DotTray.Windows.Native.Models;
using System.Runtime.InteropServices;

internal static partial class PInvoke
{
    [LibraryImport(Shell32, EntryPoint = "Shell_NotifyIconW", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool Shell_NotifyIcon(uint message, ref NOTIFYICONDATA data);
}