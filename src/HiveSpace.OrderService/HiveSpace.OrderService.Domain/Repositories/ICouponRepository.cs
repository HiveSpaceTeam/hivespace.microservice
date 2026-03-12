using HiveSpace.Domain.Shared.Interfaces;
using HiveSpace.OrderService.Domain.Aggregates.Coupons;

namespace HiveSpace.OrderService.Domain.Repositories;

/// <summary>
/// Repository interface for Coupon aggregate.
/// </summary>
public interface ICouponRepository : IRepository<Coupon>
{
}

