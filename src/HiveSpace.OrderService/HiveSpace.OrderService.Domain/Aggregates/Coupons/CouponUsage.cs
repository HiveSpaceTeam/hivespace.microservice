using HiveSpace.Domain.Shared.Entities;
using HiveSpace.OrderService.Domain.ValueObjects;

namespace HiveSpace.OrderService.Domain.Aggregates.Coupons;

/// <summary>
/// CouponUsage entity - tracks when and by whom a coupon was used
/// </summary>
public class CouponUsage : Entity<Guid>
{

    public Guid UserId { get; private set; }
    public Guid OrderId { get; private set; }
    public Money DiscountAmount { get; private set; } = null!;
    public DateTimeOffset UsedAt { get; private set; }

    private CouponUsage() { }

    private CouponUsage(Guid userId, Guid orderId, Money discountAmount)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        OrderId = orderId;
        DiscountAmount = discountAmount;
        UsedAt = DateTimeOffset.UtcNow;
    }

    public static CouponUsage Create(Guid userId, Guid orderId, Money discountAmount)
    {
        return new CouponUsage(userId, orderId, discountAmount);
    }
}
