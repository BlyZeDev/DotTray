namespace DotTray.Direct.Typography;

using System.Globalization;

/// <summary>
/// Represents a validated BCP47 language tag
/// </summary>
public readonly record struct LanguageTag
{
    /// <summary>
    /// The validated BCP47 language tag value, eg. en-US
    /// </summary>
    public readonly string Value { get; }

    /// <summary>
    /// Initializes a new <see cref="LanguageTag"/>
    /// </summary>
    /// <param name="tag">The name of the culture in BCP47</param>
    /// <exception cref="ArgumentException"></exception>
    public LanguageTag(string tag) : this(FromTag(tag)) { }

    /// <summary>
    /// Initializes a new <see cref="LanguageTag"/>
    /// </summary>
    /// <param name="info">The culture info to create the tag from</param>
    public LanguageTag(CultureInfo info) => Value = info.Name;

    /// <inheritdoc/>
    public override string ToString() => Value;

    private static CultureInfo FromTag(string? tag)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tag);

        try
        {
            var culture = CultureInfo.GetCultureInfo(tag);
            return culture;
        }
        catch (Exception ex) when (ex is ArgumentException or CultureNotFoundException)
        {
            throw new ArgumentException($"'{tag}' is not a valid or supported language tag.", nameof(tag), ex);
        }
    }
}