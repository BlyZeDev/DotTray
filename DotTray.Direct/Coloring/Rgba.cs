namespace DotTray.Direct.Coloring;

using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Represents a RGB color
/// </summary>
public readonly record struct Rgba
{
    private const byte Min = byte.MinValue;
    private const byte Max = byte.MaxValue;

    /// <summary>
    /// Represents a fully transparent color
    /// </summary>
    /// <remarks>
    /// Red = 0<br/>
    /// Green = 0<br/>
    /// Blue = 0<br/>
    /// Alpha = 0
    /// </remarks>
    public static readonly Rgba Transparent = new Rgba(Min, Min, Min, Min);

    /// <summary>
    /// Represents the color black
    /// </summary>
    /// <remarks>
    /// Red = 0<br/>
    /// Green = 0<br/>
    /// Blue = 0<br/>
    /// Alpha = 255
    /// </remarks>
    public static readonly Rgba Black = new Rgba(Min, Min, Min);

    /// <summary>
    /// Represents the color white
    /// </summary>
    /// <remarks>
    /// Red = 255<br/>
    /// Green = 255<br/>
    /// Blue = 255<br/>
    /// Alpha = 255
    /// </remarks>
    public static readonly Rgba White = new Rgba(Max, Max, Max);

    /// <summary>
    /// Represents the color red
    /// </summary>
    /// <remarks>
    /// Red = 255<br/>
    /// Green = 0<br/>
    /// Blue = 0<br/>
    /// Alpha = 255
    /// </remarks>
    public static readonly Rgba Red = new Rgba(Max, Min, Min);

    /// <summary>
    /// Represents the color green
    /// </summary>
    /// <remarks>
    /// Red = 0<br/>
    /// Green = 255<br/>
    /// Blue = 0<br/>
    /// Alpha = 255
    /// </remarks>
    public static readonly Rgba Green = new Rgba(Min, Max, Min);

    /// <summary>
    /// Represents the color blue
    /// </summary>
    /// <remarks>
    /// Red = 0<br/>
    /// Green = 0<br/>
    /// Blue = 255<br/>
    /// Alpha = 255
    /// </remarks>
    public static readonly Rgba Blue = new Rgba(Min, Min, Max);

    /// <summary>
    /// The red component of this <see cref="Rgba"/> instance
    /// </summary>
    public required readonly byte R { get; init; }
    /// <summary>
    /// The green component of this <see cref="Rgba"/> instance
    /// </summary>
    public required readonly byte G { get; init; }
    /// <summary>
    /// The blue component of this <see cref="Rgba"/> instance
    /// </summary>
    public required readonly byte B { get; init; }
    /// <summary>
    /// The alpha component of this <see cref="Rgba"/> instance
    /// </summary>
    public required readonly byte A { get; init; }

    /// <summary>
    /// Initializes a new <see cref="Rgba"/> instance
    /// </summary>
    /// <param name="r">The red component of the color</param>
    /// <param name="g">The green component of the color</param>
    /// <param name="b">The blue component of the color</param>
    /// <param name="a">The alpha component of the color</param>
    [SetsRequiredMembers]
    public Rgba(byte r, byte g, byte b, byte a = Max)
    {
        R = r;
        G = g;
        B = b;
        A = a;
    }

    /// <summary>
    /// Creates a <see cref="Rgba"/> value from a HEX value
    /// </summary>
    /// <remarks>
    /// Supported HEX formats: <b>#RRGGBBAA</b>, <b>#RRGGBB</b> (with or without leading '#')
    /// </remarks>
    /// <param name="hex">The hex string to translate</param>
    /// <returns><see cref="Rgba"/></returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="FormatException"></exception>
    public static Rgba FromHex(string hex)
    {
        ArgumentNullException.ThrowIfNull(hex);

        var span = hex.AsSpan();

        if (span.Length > 0 && span[0] == '#') span = span[1..];
        if (span.Length is not (6 or 8)) throw new FormatException("Hex string must be in #RRGGBB or #RRGGBBAA format");
        
        if (!byte.TryParse(span[0..2], System.Globalization.NumberStyles.HexNumber, null, out byte r) ||
            !byte.TryParse(span[2..4], System.Globalization.NumberStyles.HexNumber, null, out byte g) ||
            !byte.TryParse(span[4..6], System.Globalization.NumberStyles.HexNumber, null, out byte b))
            throw new FormatException("Invalid hex color components");

        var a = Max;
        if (span.Length == 8)
        {
            if (!byte.TryParse(span[6..8], System.Globalization.NumberStyles.HexNumber, null, out a))
                throw new FormatException("Invalid alpha component in hex string");
        }

        return new Rgba(r, g, b, a);
    }
}