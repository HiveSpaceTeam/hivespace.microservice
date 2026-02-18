using HiveSpace.Domain.Shared.Entities;
using HiveSpace.OrderService.Domain.Enumerations;
using HiveSpace.OrderService.Domain.ValueObjects;

namespace HiveSpace.OrderService.Domain.Aggregates.Orders;

public class Discount : Entity<Guid>
{
    public Guid CouponId { get; private set; }
    public string CouponCode { get; private set; } = null!;
    public Money DiscountAmount { get; private set; } = null!;
    public CouponScope Scope { get; private set; } = null!;
    public CouponOwnerType CouponOwnerType { get; private set; }
    public DateTimeOffset AppliedAt { get; private set; }

    private Discount() { }

    public static Discount CreateStoreDiscount(Guid couponId, string couponCode, Money discountAmount, CouponScope scope)
    {
        return new Discount
        {
            Id = Guid.NewGuid(),
            CouponId = couponId,
            CouponCode = couponCode,
            DiscountAmount = discountAmount,
            Scope = scope,
            CouponOwnerType = CouponOwnerType.Store,
            AppliedAt = DateTimeOffset.UtcNow
        };
    }

    public static Discount CreatePlatformDiscount(Guid couponId, string couponCode, Money discountAmount, CouponScope scope)
    {
        return new Discount
        {
            Id = Guid.NewGuid(),
            CouponId = couponId,
            CouponCode = couponCode,
            DiscountAmount = discountAmount,
            Scope = scope,
            CouponOwnerType = CouponOwnerType.Platform,
            AppliedAt = DateTimeOffset.UtcNow
        };
    }
}
