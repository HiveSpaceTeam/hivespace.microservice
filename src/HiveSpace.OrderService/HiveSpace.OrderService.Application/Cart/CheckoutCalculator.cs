using HiveSpace.Domain.Shared.ValueObjects;
using HiveSpace.OrderService.Domain.Aggregates.Coupons;
using HiveSpace.OrderService.Domain.Enumerations;

namespace HiveSpace.OrderService.Application.Cart;

public static class CheckoutCalculator
{
    /// <summary>
    /// Flat shipping fee based on total item quantity across the entire order.
    /// </summary>
    public static long CalculateShippingFee(int totalItemCount)
        => totalItemCount <= 5 ? 30_000L : 50_000L;

    /// <summary>
    /// Distributes a total shipping fee equally across <paramref name="storeCount"/> packages.
    /// Returns an array where index 0 receives any integer remainder.
    /// </summary>
    public static long[] DistributeShippingFee(long totalShippingFee, int storeCount)
    {
        if (storeCount <= 0) return [];

        var fees = new long[storeCount];
        var baseShipping = totalShippingFee / storeCount;
        var remainder    = totalShippingFee % storeCount;

        for (int i = 0; i < storeCount; i++)
            fees[i] = baseShipping + (i == 0 ? remainder : 0);

        return fees;
    }

    /// <summary>
    /// Applies a single coupon to a package and returns (itemDiscount, shippingDiscount).
    /// Respects <see cref="CouponScope"/>: ShippingFee coupons reduce shipping, ItemPrice coupons reduce the subtotal.
    /// Returns (0, 0) if the coupon fails validation.
    /// </summary>
    public static (long itemDiscount, long shippingDiscount) ApplyCoupon(
        Coupon coupon,
        Guid userId,
        long subtotal,
        long shippingFee,
        IEnumerable<long>? productIds = null,
        Guid? storeId = null)
    {
        var subtotalMoney = Money.FromVND(subtotal);
        var validation    = coupon.Validate(userId, subtotalMoney, productIds, storeId);
        if (!validation.IsValid) return (0L, 0L);

        var discountAmount = coupon.CalculateDiscount(subtotalMoney).Amount;

        return coupon.Scope == CouponScope.ShippingFee
            ? (0L, Math.Min(discountAmount, shippingFee))
            : (discountAmount, 0L);
    }
}
