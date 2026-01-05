using HiveSpace.CatalogService.Domain.Aggregates.External;

namespace HiveSpace.CatalogService.Domain.Repositories.External;

public interface IStoreRefRepository
{
    Task AddAsync(StoreRef store, CancellationToken cancellationToken = default);
}
