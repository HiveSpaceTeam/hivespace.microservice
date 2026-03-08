using HiveSpace.CatalogService.Domain.Aggregates.External;
using HiveSpace.CatalogService.Domain.Repositories.External;
using HiveSpace.Infrastructure.Messaging.Shared.Events.Stores;
using MassTransit;

namespace HiveSpace.CatalogService.Api.Consumers;

public class StoreCreatedConsumer : IConsumer<StoreCreatedIntegrationEvent>
{
    private readonly ILogger<StoreCreatedConsumer> _logger;
    private readonly IStoreRefRepository _storeRefRepository;

    public StoreCreatedConsumer(IStoreRefRepository storeRepository, ILogger<StoreCreatedConsumer> logger)
    {
        _logger = logger;
        _storeRefRepository = storeRepository;
    }

    public async Task Consume(ConsumeContext<StoreCreatedIntegrationEvent> context)
    {
        _logger.LogInformation("store created event received");

        await _storeRefRepository.AddAsync(new StoreRef
        (
            context.Message.Id,
            context.Message.OwnerId,
            context.Message.StoreName,
            context.Message.Description,
            context.Message.LogoUrl,
            context.Message.Address,
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow
        ), context.CancellationToken);
    }
}

