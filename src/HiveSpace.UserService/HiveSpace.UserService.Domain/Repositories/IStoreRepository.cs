using HiveSpace.Domain.Shared.Interfaces;
using HiveSpace.UserService.Domain.Aggregates.Store;

namespace HiveSpace.UserService.Domain.Repositories;

public interface IStoreRepository : IRepository<Store>
{
    Task<Store?> GetByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken = default);
    Task<bool> StoreNameExistsAsync(string storeName, CancellationToken cancellationToken = default);
}
