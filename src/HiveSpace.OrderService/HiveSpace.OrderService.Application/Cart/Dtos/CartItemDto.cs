using HiveSpace.Domain.Shared.Enumerations;
using HiveSpace.OrderService.Domain.Enumerations;

namespace HiveSpace.OrderService.Application.Cart.Dtos;

public record CartItemDto(
    Guid CartItemId,
    long ProductId,
    long SkuId,
    int Quantity,
    bool IsSelected,
    // From ProductRef
    string? ProductName,
    string? ProductThumbnailUrl,
    ProductStatus? ProductStatus,
    // From SkuRef
    long? Price,
    string? Currency,
    string? SkuNo,
    string? SkuName,
    string? SkuImageUrl,
    string? SkuAttributes,
    // From StoreRef
    string? StoreName,
    SellerStatus? StoreStatus,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt
);
