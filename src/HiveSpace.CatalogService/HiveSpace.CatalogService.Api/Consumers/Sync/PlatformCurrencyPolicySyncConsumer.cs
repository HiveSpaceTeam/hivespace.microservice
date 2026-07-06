using HiveSpace.CatalogService.Domain.Aggregates.External;
using HiveSpace.CatalogService.Domain.Repositories.External;
using HiveSpace.Infrastructure.Messaging.Shared.Events.Users;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace HiveSpace.CatalogService.Api.Consumers.Sync;

public class PlatformCurrencyPolicySyncConsumer(
    IPlatformCurrencyPolicyRefRepository repository,
    ILogger<PlatformCurrencyPolicySyncConsumer> logger)
    : IConsumer<PlatformCurrencyPolicyUpdatedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<PlatformCurrencyPolicyUpdatedIntegrationEvent> context)
    {
        var msg = context.Message;
        var existing = await repository.GetCurrentAsync(context.CancellationToken);
        if (existing is not null && existing.Version >= msg.Version)
            return;

        var enabledCurrencies = msg.Currencies.Where(x => x.IsEnabled).Select(x => x.CurrencyCode);

        if (existing is null)
            repository.Add(new PlatformCurrencyPolicyRef(msg.PolicyId, msg.DefaultCurrencyCode, msg.Version, msg.UpdatedAt, enabledCurrencies));
        else
            existing.Update(msg.DefaultCurrencyCode, msg.Version, msg.UpdatedAt, enabledCurrencies);

        await repository.SaveChangesAsync(context.CancellationToken);
        logger.LogInformation("Platform currency policy synced. Version={Version}", msg.Version);
    }
}
