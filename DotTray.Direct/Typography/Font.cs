namespace DotTray.Direct.Typography;

using DotTray.Direct.Internal;
using DotTray.Direct.Internal.Rendering;
using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Represents a font family that is guaranteed to be available
/// </summary>
public abstract class Font : IEquatable<Font>
{
    private static readonly Lazy<IDWriteFactory3> Factory = new Lazy<IDWriteFactory3>(() =>
    {
        HR.ThrowIfError(PInvoke.DWriteCreateFactory(0, typeof(IDWriteFactory).GUID, out var factory));
        return (IDWriteFactory3)factory;
    });

    internal string Key { get; }

    /// <summary>
    /// The name of the font family
    /// </summary>
    public string Family { get; }

    internal Font(string key, string family)
    {
        Key = key;
        Family = family;
    }

    /// <inheritdoc/>
    public bool Equals(Font? other) => other is not null && Key.Equals(other.Key, StringComparison.Ordinal);

    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as Font);

    /// <inheritdoc/>
    public override int GetHashCode() => Key.GetHashCode();

    /// <inheritdoc/>
    public override string ToString() => Family;

    internal static bool SystemFamilyExists(string family)
    {
        return !HR.IsError(Factory.Value.GetSystemFontCollection(out var collection, 0))
            && !HR.IsError(collection.FindFamilyName(family, out _, out var exists))
            && exists != 0;
    }

    internal static bool TryProbeUser(string[] paths, out string[] fullPaths, out string family, [NotNullWhen(false)] out Exception? error)
    {
        fullPaths = new string[paths.Length];
        family = "";
        error = null;

        for (var i = 0; i < paths.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(paths[i]))
            {
                error = new ArgumentException("Font path must not be empty", nameof(paths));
                return false;
            }

            fullPaths[i] = Path.GetFullPath(paths[i]);

            if (!File.Exists(fullPaths[i]))
            {
                error = new FileNotFoundException("Font file not found", fullPaths[i]);
                return false;
            }

            if (!IsSupportedFontFile(fullPaths[i]))
            {
                error = new InvalidDataException($"'{fullPaths[i]}' is not a valid font file (analyze failed)");
                return false;
            }
        }

        if (!TryCreateCollection(Factory.Value, fullPaths, out var collection))
        {
            error = new InvalidDataException($"'{fullPaths[0]}' is not a valid font file (collection failed)");
            return false;
        }

        try
        {
            if (collection.GetFontFamilyCount() != 1)
            {
                error = new InvalidDataException("All font files must belong to the same font family");
                return false;
            }

            if (!TryGetFamilyName(collection, out family))
            {
                error = new InvalidDataException($"'{fullPaths[0]}' is not a valid font file (family name failed)");
                return false;
            }

            return true;
        }
        finally
        {
            Com.Release(collection);
        }
    }

    internal static bool TryCreateCollection(IDWriteFactory3 factory, string[] paths, [NotNullWhen(true)] out IDWriteFontCollection? collection)
    {
        collection = null;

        if (HR.IsError(factory.CreateFontSetBuilder(out var builder))) return false;

        try
        {
            foreach (var path in paths)
            {
                if (HR.IsError(factory.CreateFontFaceReference(path, nint.Zero, 0, 0, out var reference))) return false;

                try
                {
                    if (HR.IsError(builder.AddFontFaceReference(reference))) return false;
                }
                finally
                {
                    Com.Release(reference);
                }
            }

            if (HR.IsError(builder.CreateFontSet(out var set))) return false;

            try
            {
                return !HR.IsError(factory.CreateFontCollectionFromFontSet(set, out collection));
            }
            finally
            {
                Com.Release(set);
            }
        }
        finally
        {
            Com.Release(builder);
        }
    }

    private static bool IsSupportedFontFile(string path)
    {
        if (HR.IsError(Factory.Value.CreateFontFileReference(path, nint.Zero, out var file))) return false;

        try
        {
            return !HR.IsError(file.Analyze(out var supported, out _, out _, out _)) && supported != 0;
        }
        finally
        {
            Com.Release(file);
        }
    }

    private static unsafe bool TryGetFamilyName(IDWriteFontCollection collection, out string name)
    {
        name = "";

        if (HR.IsError(collection.GetFontFamily(0, out var family))) return false;

        try
        {
            if (HR.IsError(family.GetFamilyNames(out var names))) return false;

            try
            {
                if (HR.IsError(names.FindLocaleName("en-us", out var index, out var exists)) || exists == 0) index = 0;
                if (HR.IsError(names.GetStringLength(index, out var length))) return false;

                var buffer = new char[length + 1];

                fixed (char* pointer = buffer)
                {
                    if (HR.IsError(names.GetString(index, pointer, length + 1))) return false;
                }

                name = new string(buffer, 0, (int)length);
                return name.Length > 0;
            }
            finally
            {
                Com.Release(names);
            }
        }
        finally
        {
            Com.Release(family);
        }
    }
}