using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.Domain.Shared.ValueObjects;
using HiveSpace.OrderService.Domain.Exceptions;
using HiveSpace.OrderService.Domain.Repositories;

namespace HiveSpace.OrderService.Application.Orders;

public static class CheckoutCouponUsageRecorder
{
    public static async Task CommitAsync(
        IReadOnlyCollection<OrderCouponUsageEntry> orderCouponUsages,
        ICouponRepository couponRepository,
        CancellationToken cancellationToken)
    {
        var couponCodes = orderCouponUsages
            .Select(x => x.CouponCode)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (couponCodes.Count == 0)
            return;

        var couponsByCode = (await couponRepository.GetByCodesAsync(couponCodes, cancellationToken))
            .ToDictionary(x => x.Code, StringComparer.OrdinalIgnoreCase);

        foreach (var usage in orderCouponUsages)
        {
            if (!couponsByCode.TryGetValue(usage.CouponCode, out var coupon))
                throw new NotFoundException(OrderDomainErrorCode.CouponNotFound, usage.CouponCode);

            var discountAmount = new Money(usage.DiscountAmount, usage.Currency);
            coupon.MarkAsUsed(usage.UserId, usage.OrderId, discountAmount);
        }
    }

    public static async Task ReleaseAsync(
        IReadOnlyCollection<OrderCouponUsageEntry> orderCouponUsages,
        ICouponRepository couponRepository,
        CancellationToken cancellationToken)
    {
        var couponCodes = orderCouponUsages
            .Select(x => x.CouponCode)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (couponCodes.Count == 0)
            return;

        var couponsByCode = (await couponRepository.GetByCodesAsync(couponCodes, cancellationToken))
            .ToDictionary(x => x.Code, StringComparer.OrdinalIgnoreCase);

        foreach (var usage in orderCouponUsages)
        {
            if (!couponsByCode.TryGetValue(usage.CouponCode, out var coupon))
                throw new NotFoundException(OrderDomainErrorCode.CouponNotFound, usage.CouponCode);

            coupon.ReleaseUsage(usage.OrderId);
        }
    }
}
