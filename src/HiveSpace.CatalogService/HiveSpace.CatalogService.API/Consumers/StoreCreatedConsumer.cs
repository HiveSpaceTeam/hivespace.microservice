using HiveSpace.CatalogService.Domain.Aggregates.External;
using HiveSpace.CatalogService.Domain.Repositories.External;
using HiveSpace.Infrastructure.Messaging.Shared.Events.Stores;
using MassTransit;

namespace HiveSpace.CatalogService.API.Consumers;

public class StoreCreatedConsumer : IConsumer<StoreCreatedIntegrationEvent>
{
    private readonly ILogger<StoreCreatedConsumer> _logger;
    private readonly IStoreSnapshotRepository _storeSnapshotRepository;

    public StoreCreatedConsumer(IStoreSnapshotRepository storeRepository, ILogger<StoreCreatedConsumer> logger)
    {
        _logger = logger;
        _storeSnapshotRepository = storeRepository;
    }

    public async Task Consume(ConsumeContext<StoreCreatedIntegrationEvent> context)
    {
        _logger.LogInformation("store created event received");

        await _storeSnapshotRepository.AddAsync(new StoreRef
        {
            OwnerId = context.Message.OwnerId,
            StoreName = context.Message.StoreName,
            Description = context.Message.Description,
            LogoUrl = context.Message.LogoUrl,
            Address = context.Message.Address,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        }, context.CancellationToken);
    }
}

