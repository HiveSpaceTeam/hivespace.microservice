using HiveSpace.Domain.Shared.Errors;
using HiveSpace.Domain.Shared.Exceptions;

namespace HiveSpace.Domain.Shared.Enumerations;

public enum Currency
{
    VND = 704,
    USD = 840,
    EUR = 978
}

public static class CurrencyExtensions
{
    public static string GetCode(this Currency currency)
    {
        return currency switch
        {
            Currency.VND => "VND",
            Currency.USD => "USD",
            Currency.EUR => "EUR",
            _ => throw new InvalidFieldException(DomainErrorCode.InvalidEnumerationValue, nameof(currency))
        };
    }
    
    public static Currency FromCode(string code)
    {
        return code?.ToUpperInvariant() switch
        {
            "VND" => Currency.VND,
            "USD" => Currency.USD,
            "EUR" => Currency.EUR,
            _ => throw new InvalidFieldException(DomainErrorCode.InvalidEnumerationValue, nameof(code))
        };
    }
}
