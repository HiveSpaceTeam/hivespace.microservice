using HiveSpace.Infrastructure.Messaging.Abstractions;
using HiveSpace.Infrastructure.Messaging.Shared.Events.Stores;
using HiveSpace.UserService.Application.Interfaces.Messaging;
using HiveSpace.UserService.Domain.Aggregates.Store;

namespace HiveSpace.UserService.Infrastructure.Messaging.Publishers;

public class StoreEventPublisher : IStoreEventPublisher
{
    private readonly IEventPublisher _eventPublisher;

    public StoreEventPublisher(IEventPublisher eventPublisher)
    {
        _eventPublisher = eventPublisher;
    }

    public Task PublishStoreCreatedAsync(Store store, CancellationToken cancellationToken = default)
    {
        var evt = new StoreCreatedIntegrationEvent(store.Id, store.OwnerId, store.StoreName, store.Description, store.LogoUrl, store.Address);
        return _eventPublisher.PublishAsync(evt, cancellationToken);
    }

}

