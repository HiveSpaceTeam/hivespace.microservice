using HiveSpace.Application.Shared.Queries;
using HiveSpace.OrderService.Application.Cart.Dtos;

namespace HiveSpace.OrderService.Application.Cart.Queries.GetCheckoutPreview;

public record GetCheckoutPreviewQuery(
    List<StoreCouponEntry>? StoreCoupons,
    List<string>? PlatformCouponCodes
) : IQuery<CheckoutPreviewResponse>;
