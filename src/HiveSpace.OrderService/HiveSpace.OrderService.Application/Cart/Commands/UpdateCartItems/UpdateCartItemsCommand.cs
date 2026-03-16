using MediatR;

namespace HiveSpace.OrderService.Application.Cart.Commands.UpdateCartItems;

public record 
CartItemUpdateRequest(Guid CartItemId, long? SkuId, int? Quantity, bool? IsSelected);

public record UpdateCartItemsCommand : IRequest
{
    public bool? SelectAll { get; init; }
    public List<CartItemUpdateRequest> Items { get; init; } = [];
}
