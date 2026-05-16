using HiveSpace.Domain.Shared.Enumerations;

namespace HiveSpace.OrderService.Domain.Repositories;

public record OrderCouponUsageEntry(
    Guid OrderId,
    Guid UserId,
    string CouponCode,
    long DiscountAmount,
    Currency Currency);
