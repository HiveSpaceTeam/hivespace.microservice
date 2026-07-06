namespace HiveSpace.UserService.Application.Configuration.Dtos;

public record PlatformCurrencyConfigDto(
    IReadOnlyCollection<PlatformCurrencyItemDto> Currencies,
    string DefaultCurrencyCode,
    IReadOnlyCollection<string> SupportedCurrencyCodes,
    long Version,
    DateTimeOffset? UpdatedAt);

public record PlatformCurrencyItemDto(string CurrencyCode, bool IsEnabled);
