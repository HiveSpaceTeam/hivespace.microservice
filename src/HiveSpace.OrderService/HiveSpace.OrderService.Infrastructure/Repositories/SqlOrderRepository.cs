using HiveSpace.Infrastructure.Persistence.Repositories;
using HiveSpace.OrderService.Domain.Aggregates.Orders;
using HiveSpace.OrderService.Domain.Repositories;
using HiveSpace.OrderService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HiveSpace.OrderService.Infrastructure.Repositories;

public class SqlOrderRepository(OrderDbContext db)
    : BaseRepository<Order, Guid>(db), IOrderRepository
{
    public async Task<Order?> GetByIdAsync(Guid orderId, CancellationToken ct = default)
        => await db.Orders.FirstOrDefaultAsync(o => o.Id == orderId, ct);

    public async Task<Order?> GetByIdWithPackagesAsync(Guid orderId, CancellationToken ct = default)
        => await db.Orders
            .Include(o => o.Packages)
                .ThenInclude(p => p.Items)
            .Include(o => o.Trackings)
            .FirstOrDefaultAsync(o => o.Id == orderId, ct);

    public async Task<Order?> GetByShortIdAsync(string shortId, CancellationToken ct = default)
        => await db.Orders
            .Include(o => o.Packages)
            .FirstOrDefaultAsync(o => o.ShortId == shortId, ct);

    public async Task<Order?> GetOrderByPackageIdAsync(Guid packageId, CancellationToken ct = default)
        => await db.Orders
            .Include(o => o.Packages)
                .ThenInclude(p => p.Items)
            .FirstOrDefaultAsync(o => o.Packages.Any(p => p.Id == packageId), ct);
}
