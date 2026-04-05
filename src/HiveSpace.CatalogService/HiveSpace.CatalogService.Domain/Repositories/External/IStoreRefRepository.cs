using HiveSpace.CatalogService.Domain.Aggregates.External;

namespace HiveSpace.CatalogService.Domain.Repositories.External;

public interface IStoreRefRepository
{
    Task<StoreRef?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(StoreRef store, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
