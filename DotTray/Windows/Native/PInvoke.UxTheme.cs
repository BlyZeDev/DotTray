namespace DotTray.Windows.Native;

using System.Runtime.InteropServices;

internal static partial class PInvoke
{
    [LibraryImport(UxTheme, EntryPoint = "#132")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool AllowDarkModeForApp([MarshalAs(UnmanagedType.Bool)] bool allow);

    [LibraryImport(UxTheme, EntryPoint = "#133")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool AllowDarkModeForWindow(nint hWnd, [MarshalAs(UnmanagedType.Bool)] bool allow);

    [LibraryImport(UxTheme, EntryPoint = "#135")]
    public static partial int SetPreferredAppMode(int appMode);

    [LibraryImport(UxTheme, EntryPoint = "#136")]
    public static partial void FlushMenuThemes();
}