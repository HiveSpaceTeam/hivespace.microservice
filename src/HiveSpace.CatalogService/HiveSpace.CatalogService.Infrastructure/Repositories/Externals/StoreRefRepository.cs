using HiveSpace.CatalogService.Domain.Aggregates.External;
using HiveSpace.CatalogService.Domain.Repositories.External;
using HiveSpace.CatalogService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HiveSpace.CatalogService.Infrastructure.Repositories.Externals;

public class StoreRefRepository(CatalogDbContext context) : IStoreRefRepository
{
    public Task<StoreRef?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => context.StoreRef.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public async Task AddAsync(StoreRef store, CancellationToken cancellationToken = default)
    {
        await context.StoreRef.AddAsync(store, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => context.SaveChangesAsync(cancellationToken);
}
