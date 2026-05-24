using HiveSpace.UserService.Domain.Aggregates.Store;

namespace HiveSpace.UserService.Application.Interfaces.Messaging;

public interface IStoreEventPublisher
{
    Task PublishStoreCreatedAsync(Store store, CancellationToken cancellationToken = default);
    Task PublishStoreUpdatedAsync(Store store, CancellationToken cancellationToken = default);
}
