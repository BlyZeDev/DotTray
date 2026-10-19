namespace DotTray.Direct.Internal.Rendering;

using System.Runtime.InteropServices;

internal static class Com
{
    public static void Release(object? comObject)
    {
        if (comObject is null) return;

        if (comObject is IDisposable disposable) disposable.Dispose();
        else if (Marshal.IsComObject(comObject)) Marshal.FinalReleaseComObject(comObject);
    }
}