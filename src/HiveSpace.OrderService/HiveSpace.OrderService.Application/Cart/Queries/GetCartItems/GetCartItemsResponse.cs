using HiveSpace.Core.Models.Pagination;
using HiveSpace.OrderService.Application.Cart.Dtos;

namespace HiveSpace.OrderService.Application.Cart.Queries.GetCartItems;

public record GetCartItemsResponse(
    List<CartItemDto> Items,
    PaginationMetadata Pagination
);
