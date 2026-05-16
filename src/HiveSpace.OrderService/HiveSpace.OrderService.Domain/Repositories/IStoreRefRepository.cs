using HiveSpace.OrderService.Domain.External;

namespace HiveSpace.OrderService.Domain.Repositories;

public interface IStoreRefRepository
{
    Task<StoreRef?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
