using HiveSpace.OrderService.Domain.Repositories;
using HiveSpace.OrderService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HiveSpace.OrderService.Infrastructure.Repositories;

public class SqlSkuRefRepository(OrderDbContext context) : ISkuRefRepository
{
    public Task<bool> ExistsAsync(long skuId, long productId, CancellationToken cancellationToken = default)
        => context.SkuRefs.AnyAsync(s => s.Id == skuId && s.ProductId == productId, cancellationToken);
}
