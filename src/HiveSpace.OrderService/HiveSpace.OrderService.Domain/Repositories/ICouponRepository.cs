using HiveSpace.Domain.Shared.Interfaces;
using HiveSpace.Domain.Shared.Specifications;
using HiveSpace.OrderService.Domain.Aggregates.Coupons;

namespace HiveSpace.OrderService.Domain.Repositories;

public interface ICouponRepository : IRepository<Coupon>
{
    Task<List<Coupon>> GetByCodesAsync(IEnumerable<string> codes, CancellationToken ct = default);
    Task<List<Coupon>> GetListWithUsagesAsync(Specification<Coupon> specification, CancellationToken ct = default);
}
