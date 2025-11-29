using HiveSpace.CatalogService.Application.Models.ReadModels;

namespace HiveSpace.CatalogService.Application.Interfaces.Repositories.Snapshot;

public interface IStoreSnapshotRepository
{
    Task AddAsync(StoreSnapshot store, CancellationToken cancellationToken = default);
}
