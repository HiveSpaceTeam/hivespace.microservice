using HiveSpace.Domain.Shared.Entities;
using HiveSpace.Domain.Shared.Enumerations;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.UserService.Domain.Exceptions;

namespace HiveSpace.UserService.Domain.Aggregates.Configuration;

public class PlatformCurrency : AggregateRoot<Guid>
{
    public string ConfigType { get; private set; } = null!;
    public string CurrencyCode { get; private set; } = null!;
    public bool IsEnabled { get; private set; }
    public int SortOrder { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    private PlatformCurrency()
    {
    }

    private PlatformCurrency(Guid id, string currencyCode, bool isEnabled, int sortOrder)
    {
        ValidateSupportedCode(currencyCode);

        Id = id;
        ConfigType = PlatformConfig.CurrencyConfigType;
        CurrencyCode = currencyCode.ToUpperInvariant();
        IsEnabled = isEnabled;
        SortOrder = sortOrder;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public static PlatformCurrency CreateCurrency(string currencyCode, bool isEnabled, int sortOrder)
        => new(Guid.NewGuid(), currencyCode, isEnabled, sortOrder);

    public void SetEnabled(bool isEnabled)
    {
        IsEnabled = isEnabled;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public static IReadOnlyCollection<string> GetSupportedCodes() => CurrencyExtensions.GetSupportedCodes();

    public static void ValidateSupportedCode(string currencyCode)
    {
        if (string.IsNullOrWhiteSpace(currencyCode))
            throw new InvalidFieldException(UserDomainErrorCode.PlatformCurrencyUnsupported, nameof(currencyCode));

        if (!CurrencyExtensions.GetSupportedCodes().Contains(currencyCode.ToUpperInvariant(), StringComparer.OrdinalIgnoreCase))
            throw new InvalidFieldException(UserDomainErrorCode.PlatformCurrencyUnsupported, nameof(currencyCode));
    }
}
