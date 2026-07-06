using HiveSpace.Infrastructure.Messaging.Abstractions;
using HiveSpace.Infrastructure.Messaging.Shared.Events.Users;
using HiveSpace.UserService.Application.Interfaces.Messaging;
using HiveSpace.UserService.Domain.Aggregates.Configuration;

namespace HiveSpace.UserService.Infrastructure.Messaging.Publishers;

public class PlatformCurrencyConfigEventPublisher(IEventPublisher eventPublisher) : IPlatformCurrencyConfigEventPublisher
{
    public Task PublishPolicyUpdatedAsync(
        PlatformConfig config,
        IReadOnlyCollection<PlatformCurrency> currencies,
        CancellationToken cancellationToken = default)
    {
        var evt = new PlatformCurrencyPolicyUpdatedIntegrationEvent
        {
            PolicyId = config.Id,
            DefaultCurrencyCode = config.DefaultCurrencyCode,
            Version = config.Version,
            UpdatedAt = config.UpdatedAt,
            Currencies = currencies
                .OrderBy(x => x.SortOrder)
                .Select(x => new PlatformCurrencyPolicyUpdatedIntegrationEvent.CurrencyPolicyItem
                {
                    CurrencyCode = x.CurrencyCode,
                    IsEnabled = x.IsEnabled
                })
                .ToArray()
        };

        return eventPublisher.PublishAsync(evt, cancellationToken);
    }
}
