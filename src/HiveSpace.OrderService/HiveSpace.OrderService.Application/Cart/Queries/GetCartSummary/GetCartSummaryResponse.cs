using HiveSpace.OrderService.Application.Cart.Dtos;

namespace HiveSpace.OrderService.Application.Cart.Queries.GetCartSummary;

public record GetCartSummaryResponse(
    List<CartStoreGroupDto> Stores,
    CartSummaryTotalsResponse Summary,
    List<AppliedPlatformCouponDto> PlatformCoupons,
    List<InvalidAppliedCouponDto> InvalidatedCoupons,
    bool HasMore
);
