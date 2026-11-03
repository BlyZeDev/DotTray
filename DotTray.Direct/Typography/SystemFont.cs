namespace DotTray.Direct.Typography;

using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Represents a font installed on the system
/// </summary>
public sealed class SystemFont : Font
{
    private static readonly Lazy<SystemFont> DefaultFont = new Lazy<SystemFont>(() =>
        TryCreate("Segoe UI", out var font) || TryCreate("Arial", out font) ? font : throw new FontNotFoundException("Segoe UI"));

    /// <summary>
    /// The default UI font (Segoe UI, or Arial if it's not installed)
    /// </summary>
    /// <remarks>
    /// This throws a <see cref="FontNotFoundException"/>, if Segoe UI nor Arial are found
    /// </remarks>
    /// <exception cref="FontNotFoundException"></exception>
    public static SystemFont Default => DefaultFont.Value;

    /// <summary>
    /// Initializes a new <see cref="SystemFont"/> instance
    /// </summary>
    /// <param name="family">The family name of an installed font</param>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="FontNotFoundException"></exception>
    public SystemFont(string family) : base("system:" + family.ToLowerInvariant(), Validate(family)) { }

    /// <summary>
    /// Checks whether a font family is installed on the system
    /// </summary>
    public static bool Exists(string family) => !string.IsNullOrWhiteSpace(family) && SystemFamilyExists(family);

    /// <summary>
    /// Creates a <see cref="SystemFont"/> if <paramref name="family"/> is installed
    /// </summary>
    public static bool TryCreate(string family, [NotNullWhen(true)] out SystemFont? font)
    {
        font = Exists(family) ? new SystemFont(family) : null;
        return font is not null;
    }

    private static string Validate(string family)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(family);
        return SystemFamilyExists(family) ? family : throw new FontNotFoundException(family);
    }
}