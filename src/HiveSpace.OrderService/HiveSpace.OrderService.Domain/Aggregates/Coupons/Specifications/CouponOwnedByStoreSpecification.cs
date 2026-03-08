using System;
using System.Linq.Expressions;
using HiveSpace.Domain.Shared.Specifications;

namespace HiveSpace.OrderService.Domain.Aggregates.Coupons.Specifications;

/// <summary>
/// Filters coupons that belong to a specific seller store.
/// Ensures sellers only see their own coupons.
/// </summary>
public class CouponOwnedByStoreSpecification : Specification<Coupon>
{
    private readonly Guid _storeId;

    public CouponOwnedByStoreSpecification(Guid storeId)
    {
        _storeId = storeId;
    }

    public override Expression<Func<Coupon, bool>> ToExpression()
    {
        return coupon => coupon.StoreId == _storeId;
    }
}
