using HiveSpace.Application.Shared.Events.Stores;
using HiveSpace.CatalogService.Application.Interfaces.Repositories.Snapshot;
using HiveSpace.CatalogService.Application.Models.ReadModels;
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

        await _storeSnapshotRepository.AddAsync(new StoreSnapshot
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

