namespace DotTray.Direct.Internal.Models;

using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
internal struct Matrix3x2
{
    public float M11, M12, M21, M22, M31, M32;
}