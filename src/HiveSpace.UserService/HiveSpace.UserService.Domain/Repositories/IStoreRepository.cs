using HiveSpace.Domain.Shared.Interfaces;
using HiveSpace.UserService.Domain.Aggregates.Store;
using HiveSpace.UserService.Domain.Enums;

namespace HiveSpace.UserService.Domain.Repositories;

public interface IStoreRepository : IRepository<Store>
{
    Task<Store?> GetByStoreNameAsync(string storeName, CancellationToken cancellationToken = default);
    Task<Store?> GetByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Store>> GetByStatusAsync(StoreStatus status, CancellationToken cancellationToken = default);
    Task<bool> StoreNameExistsAsync(string storeName, CancellationToken cancellationToken = default);
    Task<IEnumerable<Store>> GetPaginatedAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default);
}
