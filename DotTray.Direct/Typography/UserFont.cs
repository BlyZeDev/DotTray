namespace DotTray.Direct.Typography;

using System.Diagnostics.CodeAnalysis;
using FamilyPaths = (string Family, string[] Paths);

/// <summary>
/// Represents a font loaded from font files
/// </summary>
public sealed class UserFont : Font
{
    /// <summary>
    /// The full paths of the font files
    /// </summary>
    public IReadOnlyList<string> Paths { get; }

    private UserFont(FamilyPaths data) : base("user:" + string.Join('|', data.Paths).ToLowerInvariant(), data.Family) => Paths = data.Paths;

    /// <summary>
    /// Initializes a new <see cref="UserFont"/>
    /// </summary>
    /// <param name="paths">The paths to font files</param>
    public UserFont(params string[] paths) : this(Load(paths)) { }

    /// <summary>
    /// <see langword="true"/> if all <paramref name="paths"/> are valid font files of one family, otherwise <see langword="false"/>
    /// </summary>
    /// <param name="paths">The paths to check</param>
    /// <returns><see langword="bool"/></returns>
    public static bool Exists(params string[] paths) => paths.Length > 0 && TryProbeUser(paths, out _, out _, out _);

    /// <summary>
    /// Creates a <see cref="UserFont"/> if the file is a valid font
    /// </summary>
    /// <param name="path">The path to create the font from</param>
    /// <param name="font">The created font or <see langword="null"/> if the file is not a valid font</param>
    /// <returns><see langword="bool"/></returns>
    public static bool TryCreate(string path, [NotNullWhen(true)] out UserFont? font) => TryCreate([path], out font);

    /// <summary>
    /// Creates a <see cref="UserFont"/> if the files are valid font files of one family
    /// </summary>
    /// <param name="paths">The paths to create the font from</param>
    /// <param name="font">The created font or <see langword="null"/> if the files are not valid font files of one family</param>
    /// <returns><see langword="bool"/></returns>
    public static bool TryCreate(string[] paths, [NotNullWhen(true)] out UserFont? font)
    {
        font = null;
        if (paths.Length < 1 || !TryProbeUser(paths, out var full, out var family, out _)) return false;

        font = new UserFont((family, full));
        return true;
    }

    private static FamilyPaths Load(string[] paths)
    {
        if (paths.Length == 0) throw new ArgumentException("At least one font file is required", nameof(paths));

        if (!TryProbeUser(paths, out var full, out var family, out var error)) throw error;

        return (family, full);
    }
}