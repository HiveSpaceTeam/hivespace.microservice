using HiveSpace.Application.Shared.Events.Stores;
using HiveSpace.CatalogService.Application.Interfaces.Repositories.Snapshot;
using HiveSpace.CatalogService.Application.Models.ReadModels;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace HiveSpace.CatalogService.Application.Consumers;

public class StoreCreatedConsumer : IConsumer<StoreCreatedIntegrationEvent>
{
    private readonly ILogger<UserCreatedConsumer> _logger;
    private readonly IStoreSnapshotRepository _storeSnapshotRepository;

    public StoreCreatedConsumer(IStoreSnapshotRepository storeRepository, ILogger<UserCreatedConsumer> logger)
    {
        _logger = logger;
        _storeSnapshotRepository = storeRepository;
    }

    public Task Consume(ConsumeContext<StoreCreatedIntegrationEvent> context)
    {
        _logger.LogInformation("store created event received");

        _storeSnapshotRepository.AddAsync(new StoreSnapshot
        {
            OwnerId = context.Message.OwnerId,
            StoreName = context.Message.StoreName,
            Description = context.Message.Description,
            LogoUrl = context.Message.LogoUrl,
            Address = context.Message.Address,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        });

        return Task.CompletedTask;
    }
}

