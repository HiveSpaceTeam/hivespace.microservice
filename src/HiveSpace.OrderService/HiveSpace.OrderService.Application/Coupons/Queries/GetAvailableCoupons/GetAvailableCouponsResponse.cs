using HiveSpace.OrderService.Application.Coupons.Dtos;

namespace HiveSpace.OrderService.Application.Coupons.Queries.GetAvailableCoupons;

public record GetAvailableCouponsResponse(
    Guid StoreId,
    string StoreName,
    string? StoreLogoUrl,
    List<AvailableCouponDto> Coupons);
