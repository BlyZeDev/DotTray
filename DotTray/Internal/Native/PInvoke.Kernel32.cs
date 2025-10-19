namespace DotTray.Internal.Native;

using System.Runtime.InteropServices;

internal static partial class PInvoke
{
    [DllImport(Kernel32, CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern nint GetModuleHandle(string? moduleName);
}