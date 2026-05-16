using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.OrderService.Domain.Exceptions;

namespace HiveSpace.OrderService.Domain.Aggregates.Carts;

public class CartAppliedPlatformCoupon
{
    public string CouponCode { get; private set; } = null!;

    private CartAppliedPlatformCoupon() { }

    private CartAppliedPlatformCoupon(string couponCode)
    {
        CouponCode = NormalizeCouponCode(couponCode);
    }

    public static CartAppliedPlatformCoupon Create(string couponCode) => new(couponCode);

    private static string NormalizeCouponCode(string couponCode)
    {
        var normalized = couponCode.Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(normalized))
            throw new InvalidFieldException(OrderDomainErrorCode.CouponInvalid, nameof(couponCode));

        return normalized;
    }
}
