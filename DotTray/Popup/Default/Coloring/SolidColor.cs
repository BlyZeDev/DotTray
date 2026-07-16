namespace DotTray.Popup.Default.Coloring;

using DotTray.Internal.Native;
using DotTray.Popup.Default;
using DotTray.Primitives;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

/// <summary>
/// Represents a RGBA color value
/// </summary>
public readonly record struct SolidColor : IColorable
{
    private const byte Min = byte.MinValue;
    private const byte Max = byte.MaxValue;

    /// <summary>
    /// Red=0<br/>Green=0<br/>Blue=0<br/>Alpha=0
    /// </summary>
    public static readonly SolidColor Transparent = new SolidColor(Min, Min, Min, Min);
    /// <summary>
    /// Red=0<br/>Green=0<br/>Blue=0<br/>Alpha=255
    /// </summary>
    public static readonly SolidColor Black = new SolidColor(Min, Min, Min);
    /// <summary>
    /// Red=127<br/>Green=127<br/>Blue=127<br/>Alpha=255
    /// </summary>
    public static readonly SolidColor Gray = new SolidColor(Max / 2, Max / 2, Max / 2);
    /// <summary>
    /// Red=255<br/>Green=255<br/>Blue=255<br/>Alpha=255
    /// </summary>
    public static readonly SolidColor White = new SolidColor(Max, Max, Max);
    /// <summary>
    /// Red=255<br/>Green=0<br/>Blue=0<br/>Alpha=255
    /// </summary>
    public static readonly SolidColor Red = new SolidColor(Max, Min, Min);
    /// <summary>
    /// Red=0<br/>Green=255<br/>Blue=0<br/>Alpha=255
    /// </summary>
    public static readonly SolidColor Green = new SolidColor(Min, Max, Min);
    /// <summary>
    /// Red=0<br/>Green=0<br/>Blue=255<br/>Alpha=255
    /// </summary>
    public static readonly SolidColor Blue = new SolidColor(Min, Min, Max);

    /// <summary>
    /// The red component
    /// </summary>
    public required readonly byte R { get; init; }

    /// <summary>
    /// The green component
    /// </summary>
    public required readonly byte G { get; init; }

    /// <summary>
    /// The blue component
    /// </summary>
    public required readonly byte B { get; init; }

    /// <summary>
    /// The alpha component
    /// </summary>
    public required readonly byte A { get; init; }

    /// <summary>
    /// Initializes a new <see cref="SolidColor"/>
    /// </summary>
    /// <param name="r">The red component</param>
    /// <param name="g">The green component</param>
    /// <param name="b">The blue component</param>
    [SetsRequiredMembers]
    public SolidColor(byte r, byte g, byte b) : this(r, g, b, Max) { }

    /// <summary>
    /// Initializes a new <see cref="SolidColor"/>
    /// </summary>
    /// <param name="r">The red component</param>
    /// <param name="g">The green component</param>
    /// <param name="b">The blue component</param>
    /// <param name="a">The alpha component</param>
    [SetsRequiredMembers]
    public SolidColor(byte r, byte g, byte b, byte a)
    {
        R = r;
        G = g;
        B = b;
        A = a;
    }

    /// <summary>
    /// Inverts a colors
    /// </summary>
    /// <param name="invertAlpha"><see langword="true"/> if <see cref="A"/> should be inverted too, otherwise <see langword="false"/></param>
    /// <returns><see cref="SolidColor"/></returns>
    public SolidColor Invert(bool invertAlpha = false)
        => new SolidColor((byte)(Max - R), (byte)(Max - G), (byte)(Max - B), invertAlpha ? (byte)(Max - A) : A);

    /// <inheritdoc/>
    public bool Equals(IColorable? other) => other is SolidColor solidColor && Equals(solidColor);

    SafeHandle IColorable.CreateNativeHandle(RectangleF bounds)
    {
        PInvoke.GdipCreateSolidFill((uint)(A << 24 | R << 16 | G << 8 | B), out var hBrush);
        return new ColorSafeHandle(hBrush);
    }

    /// <summary>
    /// Creates a <see cref="SolidColor"/> from a HEX color string in the format #RRGGBB or #RRGGBBAA
    /// </summary>
    /// <param name="hexString">The HEX color string in format #RRGGBB or #RRGGBBAA</param>
    /// <returns><see cref="SolidColor"/></returns>
    /// <exception cref="FormatException"></exception>
    public static SolidColor FromHex(string hexString) => FromHex(hexString);

    /// <summary>
    /// Creates a <see cref="SolidColor"/> from a HEX color string in the format #RRGGBB or #RRGGBBAA
    /// </summary>
    /// <param name="hexString">The HEX color string in format #RRGGBB or #RRGGBBAA</param>
    /// <returns><see cref="SolidColor"/></returns>
    /// <exception cref="FormatException"></exception>
    public static SolidColor FromHex(ReadOnlySpan<char> hexString)
    {
        if (hexString.Length is not (7 or 9) || hexString[0] != '#') throw new FormatException("HEX color needs to be in the format #RRGGBB or #RRGGBBAA");

        if (!uint.TryParse(hexString[1..], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var hex)) throw new FormatException("HEX color needs to be in the format #RRGGBB or #RRGGBBAA");

        byte r, g, b, a;
        if (hexString.Length == 7)
        {
            r = (byte)(hex >> 16);
            g = (byte)(hex >> 8);
            b = (byte)hex;
            a = Max;
        }
        else
        {
            r = (byte)(hex >> 24);
            g = (byte)(hex >> 16);
            b = (byte)(hex >> 8);
            a = (byte)hex;
        }

        return new SolidColor(r, g, b, a);
    }

    /// <summary>
    /// Generates a <see cref="SolidColor"/> with random RGB values.<br/>
    /// Optionally the A value can be random as well
    /// </summary>
    /// <param name="randomAlpha"><see langword="true"/> if the alpha value should be random as well, otherwise <see langword="false"/></param>
    /// <returns><see cref="SolidColor"/></returns>
    public static SolidColor Random(bool randomAlpha = false)
    {
        Span<byte> bytes = stackalloc byte[4];
        RandomNumberGenerator.Fill(bytes);
        return new SolidColor(bytes[0], bytes[1], bytes[2], randomAlpha ? bytes[3] : Max);
    }
}