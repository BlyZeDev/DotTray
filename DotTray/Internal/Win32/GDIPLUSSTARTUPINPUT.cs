namespace DotTray.Internal.Win32;

using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
internal struct GDIPLUSSTARTUPINPUT
{
    public uint GdiplusVersion;
    public nint DebugEventCallback;
    public bool SuppressBackgroundThread;
    public bool SuppressExternalCodecs;
}