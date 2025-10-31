namespace DotTray;

using System.Diagnostics.CodeAnalysis;

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

    internal uint ToGdiPlus() => (uint)(A << 24 | R << 16 | G << 8 | B);
}