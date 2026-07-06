using HiveSpace.Application.Shared.Handlers;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.UserService.Application.Configuration.Dtos;
using HiveSpace.UserService.Domain.Aggregates.Configuration;
using HiveSpace.UserService.Domain.Exceptions;
using HiveSpace.UserService.Domain.Repositories;

namespace HiveSpace.UserService.Application.Configuration.Queries.GetPlatformCurrencyConfig;

public class GetPlatformCurrencyConfigQueryHandler(
    IPlatformConfigRepository platformConfigRepository,
    IPlatformCurrencyRepository platformCurrencyRepository)
    : IQueryHandler<GetPlatformCurrencyConfigQuery, PlatformCurrencyConfigDto>
{
    public async Task<PlatformCurrencyConfigDto> Handle(GetPlatformCurrencyConfigQuery request, CancellationToken cancellationToken)
    {
        var config = await platformConfigRepository.GetCurrencyPolicyAsync(cancellationToken)
            ?? throw new NotFoundException(UserDomainErrorCode.PlatformCurrencySetRequired, nameof(PlatformConfig));
        var currencies = await platformCurrencyRepository.GetCurrencyItemsAsync(cancellationToken);

        return new PlatformCurrencyConfigDto(
            currencies.OrderBy(c => c.SortOrder).Select(c => new PlatformCurrencyItemDto(c.CurrencyCode, c.IsEnabled)).ToArray(),
            config.DefaultCurrencyCode,
            PlatformCurrency.GetSupportedCodes(),
            config.Version,
            config.UpdatedAt);
    }
}
