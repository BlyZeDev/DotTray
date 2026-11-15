namespace DotTray.Direct.Internal.Rendering;

using System.Runtime.CompilerServices;

internal static class PixelGrid
{
    private const float Epsilon = 0.0001f;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ToPixel(float dips, float scale) => (int)MathF.Floor(dips * scale + 0.5f + Epsilon);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Snap(float dips, float scale) => ToPixel(dips, scale) / scale;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ToDip(int pixels, float scale) => (int)MathF.Floor((pixels + Epsilon) / scale);
}