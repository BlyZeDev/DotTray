namespace DotTray.Primitives;

using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Represents four-sided padding where all values act as multipliers of a base size
/// </summary>
public readonly record struct Padding
{
    /// <summary>
    /// The padding of the left side of this <see cref="Padding"/> instance
    /// </summary>
    public required readonly int Left { get; init; }
    /// <summary>
    /// The padding of the top side of this <see cref="Padding"/> instance
    /// </summary>
    public required readonly int Top { get; init; }
    /// <summary>
    /// The padding of the right side of this <see cref="Padding"/> instance
    /// </summary>
    public required readonly int Right { get; init; }
    /// <summary>
    /// The padding of the bottom side of this <see cref="Padding"/> instance
    /// </summary>
    public required readonly int Bottom { get; init; }

    /// <summary>
    /// Gets the total horizontal padding (<see cref="Left"/> + <see cref="Right"/>)
    /// </summary>
    public readonly int Horizontal => Left + Right;

    /// <summary>
    /// Gets the total vertical padding (<see cref="Top"/> + <see cref="Bottom"/>).
    /// </summary>
    public readonly int Vertical => Top + Bottom;

    /// <summary>
    /// Initializes a new instance with specific values for all four sides
    /// </summary>
    /// <param name="left">The left size padding</param>
    /// <param name="top">The top size padding</param>
    /// <param name="right">The right size padding</param>
    /// <param name="bottom">The bottom size padding</param>
    [SetsRequiredMembers]
    public Padding(int left, int top, int right, int bottom)
    {
        Left = left;
        Top = top;
        Right = right;
        Bottom = bottom;
    }

    /// <summary>
    /// Initializes a new instance with a uniform value for all four sides
    /// </summary>
    /// <param name="uniform">The all side padding</param>
    [SetsRequiredMembers]
    public Padding(int uniform) : this(uniform, uniform, uniform, uniform) { }

    /// <summary>
    /// Initializes a new instance with specific horizontal and vertical values
    /// </summary>
    /// <param name="horizontal">The left and right side padding</param>
    /// <param name="vertical">The top and bottom side padding</param>
    [SetsRequiredMembers]
    public Padding(int horizontal, int vertical) : this(horizontal, vertical, horizontal, vertical) { }
}