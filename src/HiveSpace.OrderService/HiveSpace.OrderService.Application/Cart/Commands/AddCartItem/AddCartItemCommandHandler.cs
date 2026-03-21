using MediatR;
using HiveSpace.Core.Contexts;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.OrderService.Domain.Exceptions;
using HiveSpace.OrderService.Domain.External;
using HiveSpace.OrderService.Domain.Repositories;

namespace HiveSpace.OrderService.Application.Cart.Commands.AddCartItem;

public class AddCartItemCommandHandler(
    ICartRepository cartRepository,
    ISkuRefRepository skuRefRepository,
    IUserContext userContext)
    : IRequestHandler<AddCartItemCommand, Guid>
{
    public async Task<Guid> Handle(AddCartItemCommand request, CancellationToken cancellationToken)
    {
        var skuExists = await skuRefRepository.ExistsAsync(request.SkuId, request.ProductId, cancellationToken);
        if (!skuExists)
            throw new NotFoundException(OrderDomainErrorCode.CartSkuNotBelongToProduct, nameof(SkuRef));

        var userId = userContext.UserId;
        var cart = await cartRepository.GetByUserIdAsync(userId, cancellationToken);

        if (cart is null)
        {
            cart = Domain.Aggregates.Carts.Cart.Create(userId);
            cartRepository.Add(cart);
        }

        cart.AddItem(request.ProductId, request.SkuId, request.Quantity);
        await cartRepository.SaveChangesAsync(cancellationToken);

        return cart.Items.First(i => i.ProductId == request.ProductId && i.SkuId == request.SkuId).Id;
    }
}
