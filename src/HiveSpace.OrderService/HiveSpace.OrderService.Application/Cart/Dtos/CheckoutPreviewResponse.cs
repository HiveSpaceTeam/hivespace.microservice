namespace HiveSpace.OrderService.Application.Cart.Dtos;

public record CheckoutPreviewItemDto(
    Guid    CartItemId,
    long    ProductId,
    long    SkuId,
    string? ProductName,
    string? ImageUrl,
    string? SkuAttributes,
    long    OriginalPrice,
    long    Price,
    string  Currency,
    int     Quantity,
    long    LineTotal
);

public record CheckoutPreviewPackageDto(
    Guid    StoreId,
    string? StoreName,
    long    OriginalShippingFee,
    long    ShippingFee,
    string  ShippingType,
    string  Currency,
    long    OriginalSubtotal,
    long    Subtotal,
    long    PackageTotal,
    List<CheckoutPreviewItemDto> Items
);

public record CheckoutPreviewResponse(
    List<CheckoutPreviewPackageDto> Packages,
    long   OriginalSubtotal,
    long   Subtotal,
    string Currency,
    long   TotalShippingFee,
    long   GrandTotal,
    int    TotalItems
);
