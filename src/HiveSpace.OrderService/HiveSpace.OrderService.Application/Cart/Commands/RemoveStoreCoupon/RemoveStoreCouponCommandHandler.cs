using HiveSpace.Application.Shared.Handlers;
using HiveSpace.Core.Contexts;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.OrderService.Domain.Aggregates.Carts;
using HiveSpace.OrderService.Domain.Exceptions;
using HiveSpace.OrderService.Domain.Repositories;

namespace HiveSpace.OrderService.Application.Cart.Commands.RemoveStoreCoupon;

public class RemoveStoreCouponCommandHandler(
    ICartRepository cartRepository,
    IUserContext userContext)
    : ICommandHandler<RemoveStoreCouponCommand>
{
    public async Task Handle(RemoveStoreCouponCommand request, CancellationToken cancellationToken)
    {
        var cart = await cartRepository.GetByUserIdAsync(userContext.UserId, cancellationToken)
            ?? throw new NotFoundException(OrderDomainErrorCode.CartNotFound, nameof(Cart));

        cart.RemoveStoreCoupon(request.StoreId);
        await cartRepository.SaveChangesAsync(cancellationToken);
    }
}
