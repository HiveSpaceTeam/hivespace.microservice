namespace HiveSpace.OrderService.Application.Cart.Dtos;

public record CheckoutPreviewRawRow(
    Guid    CartItemId,
    long    ProductId,
    long    SkuId,
    int     Quantity,
    string? ProductName,
    string? ThumbnailUrl,
    long?   Price,
    string? Currency,
    string? SkuImageUrl,
    string? SkuAttributes,
    Guid    StoreId,
    string? StoreName
);
