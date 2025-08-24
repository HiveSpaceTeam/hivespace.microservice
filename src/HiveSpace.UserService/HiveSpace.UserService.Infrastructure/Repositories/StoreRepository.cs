using HiveSpace.Infrastructure.Persistence.Repositories;
using HiveSpace.UserService.Domain.Aggregates.Store;
using HiveSpace.UserService.Domain.Repositories;
using HiveSpace.UserService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HiveSpace.UserService.Infrastructure.Repositories;

public class StoreRepository : BaseRepository<Store, Guid>, IStoreRepository
{
    public StoreRepository(UserDbContext context) : base(context)
    {
    }

    public async Task<Store?> GetByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<Store>()
            .FirstOrDefaultAsync(s => s.OwnerId == ownerId, cancellationToken);
    }

    public async Task<bool> StoreNameExistsAsync(string storeName, CancellationToken cancellationToken = default)
    {
        return await _context.Set<Store>()
            .AnyAsync(s => s.StoreName == storeName, cancellationToken);
    }
}