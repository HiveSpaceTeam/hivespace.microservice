using System;
using System.Linq.Expressions;
using HiveSpace.Domain.Shared.Specifications;
using HiveSpace.OrderService.Domain.Enumerations;

namespace HiveSpace.OrderService.Domain.Aggregates.Coupons.Specifications;

public class CouponExpiredSpecification : Specification<Coupon>
{
    private readonly DateTimeOffset _now;

    public CouponExpiredSpecification()
    {
        _now = DateTimeOffset.UtcNow;
    }

    public override Expression<Func<Coupon, bool>> ToExpression()
    {
        // Expired coupons are either manually deactivated or past their end time
        return coupon => !coupon.IsActive 
                      || coupon.EndDateTime < _now;
    }
}
