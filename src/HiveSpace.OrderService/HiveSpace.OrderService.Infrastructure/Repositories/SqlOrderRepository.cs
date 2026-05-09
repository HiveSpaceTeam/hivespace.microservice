using HiveSpace.Infrastructure.Persistence.Repositories;
using HiveSpace.OrderService.Domain.Aggregates.Orders;
using HiveSpace.OrderService.Domain.Repositories;
using HiveSpace.OrderService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HiveSpace.OrderService.Infrastructure.Repositories;

public class SqlOrderRepository(OrderDbContext db)
    : BaseRepository<Order, Guid>(db), IOrderRepository
{
    protected override IQueryable<Order> ApplyIncludeDetail(IQueryable<Order> query)
        => query
            .Include(o => o.Items)
            .Include(o => o.Checkouts)
            .Include(o => o.Discounts)
            .Include(o => o.Trackings)
            .AsSplitQuery();

    public async Task<Order?> GetByIdAsync(Guid orderId, CancellationToken ct = default)
        => await base.GetByIdAsync(orderId, cancellationToken: ct);

    public async Task<Order?> GetDetailByIdAsync(Guid orderId, CancellationToken ct = default)
        => await base.GetByIdAsync(orderId, includeDetail: true, cancellationToken: ct);

    public async Task<Order?> GetByOrderCodeAsync(string orderCode, CancellationToken ct = default)
        => await db.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.OrderCode == orderCode, ct);

    public async Task<Order?> GetByIdAndStoreIdAsync(Guid orderId, Guid storeId, CancellationToken ct = default)
        => await db.Orders
            .Include(o => o.Items)
            .Include(o => o.Trackings)
            .AsSplitQuery()
            .FirstOrDefaultAsync(o => o.Id == orderId && o.StoreId == storeId, ct);
}
