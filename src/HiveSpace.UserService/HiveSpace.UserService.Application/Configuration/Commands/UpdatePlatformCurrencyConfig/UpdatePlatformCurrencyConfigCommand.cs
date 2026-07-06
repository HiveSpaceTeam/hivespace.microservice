using HiveSpace.Application.Shared.Commands;
using HiveSpace.UserService.Application.Configuration.Dtos;

namespace HiveSpace.UserService.Application.Configuration.Commands.UpdatePlatformCurrencyConfig;

public record UpdatePlatformCurrencyConfigCommand(
    IReadOnlyCollection<PlatformCurrencyConfigItemRequest> Currencies,
    string DefaultCurrencyCode,
    long Version) : ICommand<PlatformCurrencyConfigDto>;

public record PlatformCurrencyConfigItemRequest(string CurrencyCode, bool IsEnabled);
