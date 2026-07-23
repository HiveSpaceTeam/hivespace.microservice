using HiveSpace.Application.Shared.Dtos;

namespace HiveSpace.OrderService.Application.Cart.Dtos;

public record CheckoutPreviewItemDto(
    Guid    CartItemId,
    long    ProductId,
    long    SkuId,
    string? ProductName,
    string? ImageUrl,
    string? SkuName,
    string? SkuAttributes,
    MoneyResponseDto OriginalPrice,
    MoneyResponseDto Price,
    int     Quantity,
    MoneyResponseDto LineTotal
);

public record CheckoutPreviewPackageDto(
    Guid    StoreId,
    string? StoreName,
    MoneyResponseDto OriginalShippingFee,
    MoneyResponseDto ShippingFee,
    string  ShippingType,
    MoneyResponseDto OriginalSubtotal,
    MoneyResponseDto Subtotal,
    MoneyResponseDto PackageTotal,
    AppliedStoreCouponDto? AppliedStoreCoupon,
    List<CheckoutPreviewItemDto> Items
);

public record CheckoutPreviewResponse(
    List<CheckoutPreviewPackageDto> Packages,
    MoneyResponseDto OriginalSubtotal,
    MoneyResponseDto Subtotal,
    MoneyResponseDto TotalShippingFee,
    MoneyResponseDto GrandTotal,
    int    TotalItems,
    List<AppliedPlatformCouponDto> PlatformCoupons,
    List<InvalidAppliedCouponDto> InvalidatedCoupons
);
