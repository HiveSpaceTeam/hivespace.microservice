using HiveSpace.Infrastructure.Persistence.Repositories;
using HiveSpace.OrderService.Domain.Aggregates.Coupons;
using HiveSpace.OrderService.Domain.Repositories;
using HiveSpace.OrderService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HiveSpace.OrderService.Infrastructure.Repositories;

public class SqlCouponRepository(OrderDbContext context) : BaseRepository<Coupon, Guid>(context), ICouponRepository
{
    public async Task<List<Coupon>> GetByCodesAsync(IEnumerable<string> codes, CancellationToken ct = default)
    {
        var upperCodes = codes.Select(c => c.ToUpperInvariant()).ToList();
        return await context.Coupons
            .Include(c => c.Usages)
            .Where(c => upperCodes.Contains(c.Code))
            .ToListAsync(ct);
    }
}
