using MediatR;
using HiveSpace.Core.Contexts;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.OrderService.Domain.Aggregates.Carts;
using HiveSpace.OrderService.Domain.Exceptions;
using HiveSpace.OrderService.Domain.Repositories;

namespace HiveSpace.OrderService.Application.Cart.Commands.RemoveCartItem;

public class RemoveCartItemCommandHandler(ICartRepository cartRepository, IUserContext userContext)
    : IRequestHandler<RemoveCartItemCommand>
{
    public async Task Handle(RemoveCartItemCommand request, CancellationToken cancellationToken)
    {
        var userId = userContext.UserId;
        var cart = await cartRepository.GetByUserIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException(OrderDomainErrorCode.CartNotFound, nameof(Cart));

        cart.RemoveItemById(request.CartItemId);
        await cartRepository.SaveChangesAsync(cancellationToken);
    }
}
