namespace DotTray;

using System;

/// <summary>
/// Represents a mouse button
/// </summary>
/// <remarks>
/// This enum has flags
/// </remarks>
[Flags]
public enum MouseButton
{
    /// <summary>
    /// No mouse button
    /// </summary>
    None = 0,
    /// <summary>
    /// Left mouse button
    /// </summary>
    Left = 1 << 0,
    /// <summary>
    /// Right mouse button
    /// </summary>
    Right = 1 << 1,
    /// <summary>
    /// Middle mouse button
    /// </summary>
    Middle = 1 << 2
}