using HiveSpace.CatalogService.Domain.Aggregates.External;
using HiveSpace.CatalogService.Domain.Repositories.External;
using HiveSpace.CatalogService.Infrastructure.Data;

namespace HiveSpace.CatalogService.Infrastructure.Repositories.Domain
{
    public class StoreSnapshotRepository : IStoreSnapshotRepository
    {
        private readonly CatalogDbContext _context;
        public StoreSnapshotRepository(CatalogDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(StoreRef store, CancellationToken cancellationToken = default)
        {
            await _context.StoreSnapshots.AddAsync(store, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken); // thêm dòng này
        }
    }
}
