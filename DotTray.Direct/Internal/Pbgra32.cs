namespace DotTray.Direct.Internal;

using DotTray.Direct.Coloring;
using System.Runtime.CompilerServices;

internal readonly record struct Pbgra32
{
    private static readonly byte[] Unpremultiplied = BuildUnpremultiplyTable();

    public static readonly Pbgra32 Transparent = default;

    public readonly uint Packed { get; }

    public readonly byte A
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => (byte)(Packed >> 24);
    }

    public readonly byte R
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => (byte)(Packed >> 16);
    }

    public readonly byte G
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => (byte)(Packed >> 8);
    }

    public readonly byte B
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => (byte)Packed;
    }

    public readonly bool IsTransparent => A == byte.MinValue;
    public readonly bool IsOpaque => A == byte.MaxValue;

    public readonly uint Luma
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => (13933u * R + 46871u * G + 4732u * B + 32768u) >>> 16;
    }

    public Pbgra32(uint packed) => Packed = packed;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Pbgra32 Scale(uint factor)
    {
        var rb = (Packed & 0x00FF00FFu) * factor + 0x00800080u;
        rb = ((rb + ((rb >>> 8) & 0x00FF00FFu)) >>> 8) & 0x00FF00FFu;

        var ag = ((Packed >>> 8) & 0x00FF00FFu) * factor + 0x00800080u;
        ag = (ag + ((ag >>> 8) & 0x00FF00FFu)) & 0xFF00FF00u;

        return new Pbgra32(rb | ag);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Pbgra32 Over(Pbgra32 destination)
    {
        var a = (uint)A;

        if (a == 255) return this;
        if (a == 0) return destination;

        return destination.Scale(255 - a) + this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Rgba ToRgba()
    {
        var a = (uint)A;

        if (a == 0) return Rgba.Transparent;
        if (a == 255) return new Rgba(R, G, B, 255);

        var offset = (int)(a << 8);
        return new Rgba(Unpremultiplied[offset | R], Unpremultiplied[offset | G], Unpremultiplied[offset | B], (byte)a);
    }

    public override string ToString() => $"Pbgra32 {{ A = {A}, R = {R}, G = {G}, B = {B} }}";

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Pbgra32 FromChannels(uint a, uint r, uint g, uint b) => new Pbgra32((a << 24) | (r << 16) | (g << 8) | b);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Pbgra32 FromRgba(Rgba color)
    {
        var a = (uint)color.A;

        if (a == 255) return new(0xFF000000u | ((uint)color.R << 16) | ((uint)color.G << 8) | color.B);
        if (a == 0) return default;

        return FromChannels(a, Mul255(color.R, a), Mul255(color.G, a), Mul255(color.B, a));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint Mul255(uint x, uint y)
    {
        var t = x * y + 128;
        return (t + (t >>> 8)) >>> 8;
    }

    public static Pbgra32 operator +(Pbgra32 left, Pbgra32 right) => new(left.Packed + right.Packed);

    private static byte[] BuildUnpremultiplyTable()
    {
        var table = new byte[256 * 256];

        for (var a = 1; a < 256; a++)
        {
            for (var v = 0; v <= a; v++)
            {
                table[(a << 8) | v] = (byte)Math.Min(255, (v * 255 + a / 2) / a);
            }
        }

        return table;
    }
}