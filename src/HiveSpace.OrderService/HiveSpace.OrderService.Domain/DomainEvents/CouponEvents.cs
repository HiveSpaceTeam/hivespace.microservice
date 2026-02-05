using HiveSpace.Domain.Shared.Interfaces;
using HiveSpace.OrderService.Domain.ValueObjects;

namespace HiveSpace.OrderService.Domain.DomainEvents;

public class CouponCreatedDomainEvent(Guid couponId, string code, Money discountAmount) : IDomainEvent
{
    public Guid CouponId { get; } = couponId;
    public string Code { get; } = code;
    public Money DiscountAmount { get; } = discountAmount;
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
    public Guid EventId { get; } = Guid.NewGuid();
}

public class CouponUsedDomainEvent(Guid couponId, string code, Guid userId, Guid orderId, Money discountAmount) : IDomainEvent
{
    public Guid CouponId { get; } = couponId;
    public string Code { get; } = code;
    public Guid UserId { get; } = userId;
    public Guid OrderId { get; } = orderId;
    public Money DiscountAmount { get; } = discountAmount;
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
    public Guid EventId { get; } = Guid.NewGuid();
}

public class CouponDeactivatedDomainEvent(Guid couponId, string code) : IDomainEvent
{
    public Guid CouponId { get; } = couponId;
    public string Code { get; } = code;
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
    public Guid EventId { get; } = Guid.NewGuid();
}
