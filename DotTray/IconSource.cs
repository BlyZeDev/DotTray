namespace DotTray;

using System;
using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Represents the icon source used for a <see cref="NotifyIcon"/>
/// </summary>
public readonly struct IconSource
{
    internal string? Path { get; }
    internal nint Handle { get; }

    [MemberNotNullWhen(true, nameof(Path))]
    internal bool IsPath => Path is not null;
    internal bool IsHandle => Handle != nint.Zero;

    private IconSource(string? path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path, nameof(path));

        Path = path;
        Handle = nint.Zero;
    }

    private IconSource(nint handle)
    {
        if (handle == nint.Zero) throw new ArgumentNullException(nameof(handle), "The handle cannot be null");

        Path = null;
        Handle = handle;
    }

    /// <summary>
    /// Implicitly converts a file path to an icon source
    /// </summary>
    /// <param name="path">The path to use as icon source</param>
    public static implicit operator IconSource(string? path) => new IconSource(path);

    /// <summary>
    /// Implicitly converts an icon handle to an icon source
    /// </summary>
    /// <param name="handle">The handle to use as icon source</param>
    public static implicit operator IconSource(nint handle) => new IconSource(handle);
}