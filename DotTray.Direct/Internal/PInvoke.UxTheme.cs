namespace DotTray.Direct.Internal;

using System.Runtime.InteropServices;

internal static partial class PInvoke
{
    private const string UxTheme = "uxtheme.dll";

    [LibraryImport(UxTheme, SetLastError = true, EntryPoint = "#138")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool ShouldSystemUseDarkMode();
}