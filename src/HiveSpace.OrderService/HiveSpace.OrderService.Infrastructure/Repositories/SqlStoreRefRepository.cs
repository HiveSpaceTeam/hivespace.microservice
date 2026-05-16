using HiveSpace.OrderService.Domain.External;
using HiveSpace.OrderService.Domain.Repositories;
using HiveSpace.OrderService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HiveSpace.OrderService.Infrastructure.Repositories;

public class SqlStoreRefRepository(OrderDbContext context) : IStoreRefRepository
{
    public Task<StoreRef?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => context.StoreRefs.FirstOrDefaultAsync(store => store.Id == id, cancellationToken);
}
