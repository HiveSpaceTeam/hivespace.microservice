using HiveSpace.Domain.Shared.Interfaces;
using HiveSpace.OrderService.Domain.Aggregates.Coupons;

namespace HiveSpace.OrderService.Domain.Repositories;

public interface ICouponRepository : IRepository<Coupon>
{
    Task<List<Coupon>> GetByCodesAsync(IEnumerable<string> codes, CancellationToken ct = default);
}

