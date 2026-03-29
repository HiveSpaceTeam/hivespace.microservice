using HiveSpace.OrderService.Domain.External;
using HiveSpace.OrderService.Domain.Repositories;
using HiveSpace.OrderService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HiveSpace.OrderService.Infrastructure.Repositories;

public class SqlSkuRefRepository(OrderDbContext context) : ISkuRefRepository
{
    public Task<bool> ExistsAsync(long skuId, long productId, CancellationToken cancellationToken = default)
        => context.SkuRefs.AnyAsync(s => s.Id == skuId && s.ProductId == productId, cancellationToken);

    public async Task<List<SkuRef>> GetByIdsAsync(IEnumerable<long> ids, CancellationToken ct = default)
        => await context.SkuRefs
            .Where(s => ids.Contains(s.Id))
            .ToListAsync(ct);
}
