using System;
using System.Linq.Expressions;
using HiveSpace.Domain.Shared.Specifications;
using HiveSpace.OrderService.Domain.Enumerations;

namespace HiveSpace.OrderService.Domain.Aggregates.Coupons.Specifications;

public class CouponOngoingSpecification : Specification<Coupon>
{
    private readonly DateTimeOffset _now;

    public CouponOngoingSpecification()
    {
        _now = DateTimeOffset.UtcNow;
    }

    public override Expression<Func<Coupon, bool>> ToExpression()
    {
        return coupon => coupon.IsActive 
                      && coupon.StartDateTime <= _now 
                      && coupon.EndDateTime >= _now;
    }
}
