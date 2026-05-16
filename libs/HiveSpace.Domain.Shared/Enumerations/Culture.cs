using HiveSpace.Domain.Shared.Errors;
using HiveSpace.Domain.Shared.Exceptions;

namespace HiveSpace.Domain.Shared.Enumerations;

public enum Culture
{
    Vi = 0,
    En = 1
}

public static class CultureExtensions
{
    public static string ToCode(this Culture culture)
    {
        return culture switch
        {
            Culture.Vi => "vi",
            Culture.En => "en",
            _ => throw new InvalidFieldException(DomainErrorCode.InvalidEnumerationValue, nameof(culture))
        };
    }

    public static Culture FromCode(string code)
    {
        return code?.ToLowerInvariant() switch
        {
            "vi" => Culture.Vi,
            "en" => Culture.En,
            _ => throw new InvalidFieldException(DomainErrorCode.InvalidEnumerationValue, nameof(code))
        };
    }
}
