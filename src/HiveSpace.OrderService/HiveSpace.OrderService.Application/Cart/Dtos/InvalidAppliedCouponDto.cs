using HiveSpace.OrderService.Domain.Enumerations;

namespace HiveSpace.OrderService.Application.Cart.Dtos;

public record InvalidAppliedCouponDto(
    string CouponCode,
    CouponOwnerType OwnerType,
    Guid? StoreId,
    string ReasonCode,
    string Message
);
