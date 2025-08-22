using HiveSpace.Domain.Shared.Interfaces;
using HiveSpace.UserService.Domain.Aggregates.Store;
using HiveSpace.UserService.Domain.Enums;

namespace HiveSpace.UserService.Domain.Repositories;

public interface IStoreRepository : IRepository<Store>
{
    Task<Store?> GetByStoreNameAsync(string storeName);
    Task<Store?> GetByOwnerIdAsync(Guid ownerId);
    Task<IEnumerable<Store>> GetByStatusAsync(StoreStatus status);
    Task<bool> StoreNameExistsAsync(string storeName);
    Task<IEnumerable<Store>> GetPaginatedAsync(int page, int pageSize);
    Task<int> GetTotalCountAsync();
}
