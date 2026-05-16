using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.OrderService.Domain.Exceptions;

namespace HiveSpace.OrderService.Domain.Aggregates.Carts;

public class CartAppliedStoreCoupon
{
    public Guid StoreId { get; private set; }
    public string CouponCode { get; private set; } = null!;

    private CartAppliedStoreCoupon() { }

    private CartAppliedStoreCoupon(Guid storeId, string couponCode)
    {
        if (storeId == Guid.Empty)
            throw new InvalidFieldException(OrderDomainErrorCode.CouponStoreNotApplicable, nameof(storeId));

        StoreId = storeId;
        CouponCode = NormalizeCouponCode(couponCode);
    }

    public static CartAppliedStoreCoupon Create(Guid storeId, string couponCode) => new(storeId, couponCode);

    public void UpdateCouponCode(string couponCode)
    {
        CouponCode = NormalizeCouponCode(couponCode);
    }

    private static string NormalizeCouponCode(string couponCode)
    {
        var normalized = couponCode.Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(normalized))
            throw new InvalidFieldException(OrderDomainErrorCode.CouponInvalid, nameof(couponCode));

        return normalized;
    }
}
