using HiveSpace.CatalogService.Domain.Aggregates.External;
using HiveSpace.CatalogService.Domain.Repositories.External;
using HiveSpace.CatalogService.Infrastructure.Data;

namespace HiveSpace.CatalogService.Infrastructure.Repositories.Externals
{
    public class StoreRefRepository : IStoreRefRepository
    {
        private readonly CatalogDbContext _context;
        public StoreRefRepository(CatalogDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(StoreRef store, CancellationToken cancellationToken = default)
        {
            await _context.StoreRef.AddAsync(store, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken); // thêm dòng này
        }
    }
}
