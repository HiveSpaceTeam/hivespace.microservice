using HiveSpace.Infrastructure.Messaging.Abstractions;
using HiveSpace.Infrastructure.Messaging.Shared.Events.Stores;
using HiveSpace.UserService.Application.Interfaces.Messaging;
using HiveSpace.UserService.Domain.Aggregates.Store;

namespace HiveSpace.UserService.Infrastructure.Messaging.Publishers;

public class StoreEventPublisher(IEventPublisher eventPublisher) : IStoreEventPublisher
{
    public Task PublishStoreCreatedAsync(Store store, CancellationToken cancellationToken = default)
    {
        var evt = new StoreCreatedIntegrationEvent(
            store.Id, store.OwnerId, store.StoreName, store.Description,
            store.LogoFileId, store.LogoUrl, store.Address);
        return eventPublisher.PublishAsync(evt, cancellationToken);
    }

    public Task PublishStoreUpdatedAsync(Store store, CancellationToken cancellationToken = default)
    {
        var evt = new StoreUpdatedIntegrationEvent(
            store.Id, store.OwnerId, store.StoreName, store.Description,
            store.LogoFileId, store.LogoUrl, store.Address);
        return eventPublisher.PublishAsync(evt, cancellationToken);
    }

}
