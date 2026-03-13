using HiveSpace.Domain.Shared.IdGeneration;
using HiveSpace.Domain.Shared.Entities;
using HiveSpace.Domain.Shared.Errors;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.OrderService.Domain.Enumerations;
using HiveSpace.OrderService.Domain.Exceptions;
using HiveSpace.Domain.Shared.ValueObjects;

namespace HiveSpace.OrderService.Domain.Aggregates.Orders;

public class Discount : Entity<Guid>
{
    public Guid CouponId { get; private set; }
    public string CouponCode { get; private set; } = null!;
    public Money DiscountAmount { get; private set; } = null!;
    public CouponScope Scope { get; private set; }
    public CouponOwnerType CouponOwnerType { get; private set; }
    public DateTimeOffset AppliedAt { get; private set; }

    private Discount() { }

    public static Discount CreateStoreDiscount(Guid couponId, string couponCode, Money discountAmount, CouponScope scope)
    {
        ValidateFactoryInputs(couponId, couponCode, discountAmount);

        return new Discount
        {
            Id = IdGenerator.NewId<Guid>(),
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
        ValidateFactoryInputs(couponId, couponCode, discountAmount);

        return new Discount
        {
            Id = IdGenerator.NewId<Guid>(),
            CouponId = couponId,
            CouponCode = couponCode,
            DiscountAmount = discountAmount,
            Scope = scope,
            CouponOwnerType = CouponOwnerType.Platform,
            AppliedAt = DateTimeOffset.UtcNow
        };
    }

    private static void ValidateFactoryInputs(Guid couponId, string couponCode, Money discountAmount)
    {
        if (couponId == Guid.Empty)
            throw new InvalidFieldException(DomainErrorCode.ParameterRequired, nameof(couponId));

        if (string.IsNullOrWhiteSpace(couponCode))
            throw new InvalidFieldException(DomainErrorCode.ParameterRequired, nameof(couponCode));

        if (discountAmount is null || discountAmount.Amount <= 0)
            throw new InvalidFieldException(OrderDomainErrorCode.CouponInvalidDiscountAmount, nameof(discountAmount));
    }
}
