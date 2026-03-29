using HiveSpace.OrderService.Application.Cart.Dtos;

namespace HiveSpace.OrderService.Api.Models;

public record CheckoutPreviewRequest(
    List<StoreCouponEntry>? StoreCoupons,
    List<string>? PlatformCouponCodes
);
