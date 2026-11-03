namespace DotTray.Direct.Typography;

/// <summary>
/// Thrown when a system font family isn't installed
/// </summary>
public sealed class FontNotFoundException : Exception
{
    /// <summary>
    /// The family that wasn't found
    /// </summary>
    public string Family { get; }

    /// <inheritdoc/>
    public override string Message => $"The font family '{Family}' was not found";

    /// <summary>
    /// Initializes a new <see cref="FontNotFoundException"/>
    /// </summary>
    /// <param name="family">The font family that wasn't found</param>
    public FontNotFoundException(string family) => Family = family;
}