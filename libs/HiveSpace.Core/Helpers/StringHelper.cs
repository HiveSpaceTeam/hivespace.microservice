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
}
