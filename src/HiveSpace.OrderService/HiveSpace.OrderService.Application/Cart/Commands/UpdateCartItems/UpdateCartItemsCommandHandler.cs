using MediatR;
using HiveSpace.Core.Contexts;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.OrderService.Domain.Aggregates.Carts;
using HiveSpace.OrderService.Domain.Exceptions;
using HiveSpace.OrderService.Domain.External;
using HiveSpace.OrderService.Domain.Repositories;

namespace HiveSpace.OrderService.Application.Cart.Commands.UpdateCartItems;

public class UpdateCartItemsCommandHandler(
    ICartRepository cartRepository,
    ISkuRefRepository skuRefRepository,
    IUserContext userContext)
    : IRequestHandler<UpdateCartItemsCommand>
{
    public async Task Handle(UpdateCartItemsCommand request, CancellationToken cancellationToken)
    {
        var userId = userContext.UserId;
        var cart = await cartRepository.GetByUserIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException(OrderDomainErrorCode.CartNotFound, nameof(Cart));

        if (request.SelectAll.HasValue)
            cart.SelectAllItems(request.SelectAll.Value);

        foreach (var itemRequest in request.Items)
        {
            if (itemRequest.SkuId.HasValue)
            {
                var cartItem = cart.Items.FirstOrDefault(i => i.Id == itemRequest.CartItemId)
                    ?? throw new NotFoundException(OrderDomainErrorCode.CartItemNotFound, nameof(CartItem));

                var skuExists = await skuRefRepository.ExistsAsync(itemRequest.SkuId.Value, cartItem.ProductId, cancellationToken);
                if (!skuExists)
                    throw new NotFoundException(OrderDomainErrorCode.CartSkuNotBelongToProduct, nameof(SkuRef));
            }

            cart.UpdateItemById(itemRequest.CartItemId, itemRequest.SkuId, itemRequest.Quantity, itemRequest.IsSelected);
        }

        await cartRepository.SaveChangesAsync(cancellationToken);
    }
}
