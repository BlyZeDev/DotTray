namespace DotTray.Direct.Internal;

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

internal static class Ensure
{
    public static T EnumIsValid<T>(T value, [CallerArgumentExpression(nameof(value))] string? paramName = null) where T : struct, Enum
    {
        if (!Enum.IsDefined(value)) ThrowNotDefined(value, paramName);

        return value;
    }

    [DoesNotReturn]
    private static void ThrowNotDefined<T>(T value, string? paramName) where T : struct, Enum => throw new ArgumentOutOfRangeException(paramName, value, $"Invalid {typeof(T).Name} value.");
}