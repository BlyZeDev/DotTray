namespace DotTray.Internal.Win32;

using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
internal struct PAINTSTRUCT
{
    public nint hdc;
    public int fErase;
    public RECT rcPaint;
    public int fRestore;
    public int fIncUpdate;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
    public byte[] rgbReserved;
}