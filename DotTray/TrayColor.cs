namespace DotTray;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Security.Cryptography;

/// <summary>
/// Represents a RGBA color value
/// </summary>
public readonly record struct TrayColor
{
    private const byte Min = byte.MinValue;
    private const byte Max = byte.MaxValue;

    /// <summary>
    /// Red=0<br/>Green=0<br/>Blue=0<br/>Alpha=0
    /// </summary>
    public static readonly TrayColor Transparent = new TrayColor(Min, Min, Min, Min);
    /// <summary>
    /// Red=0<br/>Green=0<br/>Blue=0<br/>Alpha=255
    /// </summary>
    public static readonly TrayColor Black = new TrayColor(Min, Min, Min);
    /// <summary>
    /// Red=127<br/>Green=127<br/>Blue=127<br/>Alpha=255
    /// </summary>
    public static readonly TrayColor Gray = new TrayColor(Max / 2, Max / 2, Max / 2);
    /// <summary>
    /// Red=255<br/>Green=255<br/>Blue=255<br/>Alpha=255
    /// </summary>
    public static readonly TrayColor White = new TrayColor(Max, Max, Max);
    /// <summary>
    /// Red=255<br/>Green=0<br/>Blue=0<br/>Alpha=255
    /// </summary>
    public static readonly TrayColor Red = new TrayColor(Max, Min, Min);
    /// <summary>
    /// Red=0<br/>Green=255<br/>Blue=0<br/>Alpha=255
    /// </summary>
    public static readonly TrayColor Green = new TrayColor(Min, Max, Min);
    /// <summary>
    /// Red=0<br/>Green=0<br/>Blue=255<br/>Alpha=255
    /// </summary>
    public static readonly TrayColor Blue = new TrayColor(Min, Min, Max);

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
    /// Initializes a new <see cref="TrayColor"/>
    /// </summary>
    /// <param name="r">The red component</param>
    /// <param name="g">The green component</param>
    /// <param name="b">The blue component</param>
    [SetsRequiredMembers]
    public TrayColor(byte r, byte g, byte b) : this(r, g, b, Max) { }

    /// <summary>
    /// Initializes a new <see cref="TrayColor"/>
    /// </summary>
    /// <param name="r">The red component</param>
    /// <param name="g">The green component</param>
    /// <param name="b">The blue component</param>
    /// <param name="a">The alpha component</param>
    [SetsRequiredMembers]
    public TrayColor(byte r, byte g, byte b, byte a)
    {
        R = r;
        G = g;
        B = b;
        A = a;
    }

    internal readonly uint ToGdiPlus() => (uint)(A << 24 | R << 16 | G << 8 | B);

    /// <summary>
    /// Creates a <see cref="TrayColor"/> from a HEX color string in the format #RRGGBB or #RRGGBBAA
    /// </summary>
    /// <param name="hexString">The HEX color string in format #RRGGBB or #RRGGBBAA</param>
    /// <returns><see cref="TrayColor"/></returns>
    /// <exception cref="FormatException"></exception>
    public static TrayColor FromHex(string hexString) => FromHex(hexString);

    /// <summary>
    /// Creates a <see cref="TrayColor"/> from a HEX color string in the format #RRGGBB or #RRGGBBAA
    /// </summary>
    /// <param name="hexString">The HEX color string in format #RRGGBB or #RRGGBBAA</param>
    /// <returns><see cref="TrayColor"/></returns>
    /// <exception cref="FormatException"></exception>
    public static TrayColor FromHex(ReadOnlySpan<char> hexString)
    {
        if (hexString.Length is not 7 or 9 || hexString[0] != '#') throw new FormatException("HEX color needs to be in the format #RRGGBB or #RRGGBBAA");

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

        return new TrayColor(r, g, b, a);
    }

    /// <summary>
    /// Generates a <see cref="TrayColor"/> with random RGB values.<br/>
    /// Optionally the A value can be random as well
    /// </summary>
    /// <param name="randomAlpha"><see langword="true"/> if the alpha value should be random as well, otherwise <see langword="false"/></param>
    /// <returns><see cref="TrayColor"/></returns>
    public static TrayColor Random(bool randomAlpha = false)
    {
        Span<byte> bytes = stackalloc byte[4];
        RandomNumberGenerator.Fill(bytes);
        return new TrayColor(bytes[0], bytes[1], bytes[2], randomAlpha ? bytes[3] : Max);
    }
}