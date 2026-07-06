using HiveSpace.Domain.Shared.Entities;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.UserService.Domain.Exceptions;

namespace HiveSpace.UserService.Domain.Aggregates.Configuration;

public class PlatformConfig : AggregateRoot<Guid>
{
    public const string CurrencyConfigType = "currency";

    public string ConfigType { get; private set; } = null!;
    public string DefaultCurrencyCode { get; private set; } = null!;
    public long Version { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    private PlatformConfig()
    {
    }

    private PlatformConfig(Guid id, string configType, string defaultCurrencyCode, long version)
    {
        Id = id;
        ConfigType = configType;
        DefaultCurrencyCode = defaultCurrencyCode;
        Version = version;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public static PlatformConfig CreateCurrencyPolicy(string defaultCurrencyCode)
    {
        PlatformCurrency.ValidateSupportedCode(defaultCurrencyCode);
        return new PlatformConfig(Guid.NewGuid(), CurrencyConfigType, defaultCurrencyCode.ToUpperInvariant(), 1);
    }

    public void UpdateCurrencyPolicy(string defaultCurrencyCode, IReadOnlyCollection<PlatformCurrency> currencies)
    {
        PlatformCurrency.ValidateSupportedCode(defaultCurrencyCode);

        if (currencies.Count == 0 || currencies.All(c => !c.IsEnabled))
            throw new InvalidFieldException(UserDomainErrorCode.PlatformCurrencySetRequired, nameof(currencies));

        if (!currencies.Any(c => c.IsEnabled && c.CurrencyCode.Equals(defaultCurrencyCode, StringComparison.OrdinalIgnoreCase)))
            throw new ConflictException(UserDomainErrorCode.PlatformDefaultCurrencyDisabled, nameof(defaultCurrencyCode));

        DefaultCurrencyCode = defaultCurrencyCode.ToUpperInvariant();
        Version++;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
