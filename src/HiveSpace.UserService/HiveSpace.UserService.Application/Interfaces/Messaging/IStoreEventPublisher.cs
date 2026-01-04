using HiveSpace.UserService.Domain.Aggregates.Store;
using HiveSpace.UserService.Domain.Aggregates.User;

namespace HiveSpace.UserService.Application.Interfaces.Messaging;

public interface IStoreEventPublisher
{
    Task PublishStoreCreatedAsync(Store store, CancellationToken cancellationToken = default);
}

