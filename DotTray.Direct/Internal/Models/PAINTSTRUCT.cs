namespace DotTray.Direct.Internal.Models;

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
internal struct PAINTSTRUCT
{
    public nint hdc;
    public int fErase;
    public RECT rcPaint;
    public int fRestore;
    public int fIncUpdate;
    public Reserved rgbReserved;

    [InlineArray(32)]
    public struct Reserved
    {
        private byte _element0;
    }
}