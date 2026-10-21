namespace DotTray.Direct.Internal.Rendering;

using System.Runtime.InteropServices.Marshalling;

internal static class Com
{
    public static void Release(object? comObject)
    {
        switch (comObject)
        {
            case null: return;
            case IDisposable disposable: disposable.Dispose(); break;
            case ComObject com: com.FinalRelease(); break;
        }
    }
}