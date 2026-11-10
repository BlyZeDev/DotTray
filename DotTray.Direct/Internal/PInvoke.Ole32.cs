namespace DotTray.Direct.Internal;

using System.Runtime.InteropServices;

internal static partial class PInvoke
{
    private const string Ole32 = "ole32.dll";

    [LibraryImport("ole32.dll")]
    public static partial int CoInitializeEx(nint reserved, uint coInit);

    [LibraryImport("ole32.dll")]
    public static partial void CoUninitialize();

    [LibraryImport("ole32.dll")]
    public static partial int CoCreateInstance(in Guid clsid, nint outer, uint context, in Guid iid, out nint instance);
}