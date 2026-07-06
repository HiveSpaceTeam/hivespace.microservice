using HiveSpace.Application.Shared.Handlers;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.UserService.Application.Configuration.Dtos;
using HiveSpace.UserService.Application.Interfaces.Messaging;
using HiveSpace.UserService.Domain.Aggregates.Configuration;
using HiveSpace.UserService.Domain.Exceptions;
using HiveSpace.UserService.Domain.Repositories;

namespace HiveSpace.UserService.Application.Configuration.Commands.UpdatePlatformCurrencyConfig;

public class UpdatePlatformCurrencyConfigCommandHandler(
    IPlatformConfigRepository platformConfigRepository,
    IPlatformCurrencyRepository platformCurrencyRepository,
    IPlatformCurrencyConfigEventPublisher eventPublisher)
    : ICommandHandler<UpdatePlatformCurrencyConfigCommand, PlatformCurrencyConfigDto>
{
    public async Task<PlatformCurrencyConfigDto> Handle(UpdatePlatformCurrencyConfigCommand request, CancellationToken cancellationToken)
    {
        var config = await platformConfigRepository.GetCurrencyPolicyAsync(cancellationToken)
            ?? throw new NotFoundException(UserDomainErrorCode.PlatformCurrencySetRequired, nameof(PlatformConfig));

        var currencies = await platformCurrencyRepository.GetCurrencyItemsAsync(cancellationToken);

        if (config.Version != request.Version)
            throw new ConflictException(UserDomainErrorCode.PlatformCurrencyVersionConflict, nameof(PlatformConfig));

        foreach (var requested in request.Currencies)
        {
            var existing = currencies.FirstOrDefault(c => c.CurrencyCode.Equals(requested.CurrencyCode, StringComparison.OrdinalIgnoreCase))
                ?? throw new NotFoundException(UserDomainErrorCode.PlatformCurrencyUnsupported, nameof(PlatformCurrency));

            existing.SetEnabled(requested.IsEnabled);
        }

        config.UpdateCurrencyPolicy(request.DefaultCurrencyCode, currencies);

        await eventPublisher.PublishPolicyUpdatedAsync(config, currencies, cancellationToken);
        await platformConfigRepository.SaveChangesAsync(cancellationToken);

        return new PlatformCurrencyConfigDto(
            currencies.OrderBy(c => c.SortOrder).Select(c => new PlatformCurrencyItemDto(c.CurrencyCode, c.IsEnabled)).ToArray(),
            config.DefaultCurrencyCode,
            PlatformCurrency.GetSupportedCodes(),
            config.Version,
            config.UpdatedAt);
    }
}
