using HiveSpace.OrderService.Domain.External;
using HiveSpace.OrderService.Domain.Repositories;
using HiveSpace.OrderService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HiveSpace.OrderService.Infrastructure.Repositories;

public class SqlProductRefRepository(OrderDbContext context) : IProductRefRepository
{
    public async Task<List<ProductRef>> GetByIdsAsync(IEnumerable<long> ids, CancellationToken ct = default)
        => await context.ProductRefs
            .Where(p => ids.Contains(p.Id))
            .ToListAsync(ct);
}
