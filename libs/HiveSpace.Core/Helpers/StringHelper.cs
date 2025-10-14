using System.Text.Json;

namespace HiveSpace.Core.Helpers;

public static class StringHelper
{
    /// <summary>
    /// Converts an object to a string or returns an empty string if the object is null.
    /// </summary>
    public static string ToStringOrEmpty(object? value)
    {
        return value?.ToString() ?? string.Empty;
    }

    public static string? ToCamelCase(string? source)
    {
        if (string.IsNullOrEmpty(source))
            return source;

        // Use JsonNamingPolicy.CamelCase for consistent camelCase conversion
        return JsonNamingPolicy.CamelCase.ConvertName(source);
    }
}
