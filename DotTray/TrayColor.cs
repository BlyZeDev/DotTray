namespace DotTray;

using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Represents a RGBA color value
/// </summary>
public readonly record struct TrayColor
{
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
    public TrayColor(byte r, byte g, byte b) : this(r, g, b, byte.MaxValue) { }

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