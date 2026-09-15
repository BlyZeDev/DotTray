namespace DotTray.Internal.Native;

using System.Runtime.InteropServices;

internal static partial class PInvoke
{
    [LibraryImport(Kernel32, EntryPoint = "GetModuleHandleW", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
    public static partial nint GetModuleHandle(string? moduleName);

    [LibraryImport(Kernel32, SetLastError = true)]
    public static partial uint GetCurrentThreadId();
}