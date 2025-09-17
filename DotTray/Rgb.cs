namespace DotTray;

/// <summary>
/// Represents a RGB color value
/// </summary>
public readonly record struct Rgb
{
    private readonly uint _value;

    /// <summary>
    /// The red component
    /// </summary>
    public readonly byte R => (byte)(_value & 0xFF);

    /// <summary>
    /// The green component
    /// </summary>
    public readonly byte G => (byte)((_value >> 8) & 0xFF);

    /// <summary>
    /// The blue component
    /// </summary>
    public readonly byte B => (byte)((_value >> 16) & 0xFF);

    /// <summary>
    /// Initializes a RGB color value
    /// </summary>
    /// <param name="r">The red component</param>
    /// <param name="g">The green component</param>
    /// <param name="b">The blue component</param>
    public Rgb(byte r, byte g, byte b) => _value = (uint)(r | (g << 8) | (b << 16));

    /// <inheritdoc/>
    public static implicit operator uint(Rgb rgb) => rgb._value;

    /// <inheritdoc/>
    public static implicit operator int(Rgb rgb) => (int)rgb._value;
}