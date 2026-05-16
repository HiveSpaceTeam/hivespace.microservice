using HiveSpace.OrderService.Domain.Enumerations;

namespace HiveSpace.OrderService.Application.Cart.Dtos;

public record CartStoreGroupDto(
    Guid StoreId,
    string StoreName,
    SellerStatus? StoreStatus,
    bool IsMall,
    bool IsSelected,
    AppliedStoreCouponDto? AppliedStoreCoupon,
    List<CartItemDto> Items
);
