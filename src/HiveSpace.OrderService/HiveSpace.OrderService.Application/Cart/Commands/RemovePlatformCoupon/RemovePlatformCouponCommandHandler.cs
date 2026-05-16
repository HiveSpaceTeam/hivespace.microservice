using HiveSpace.Application.Shared.Handlers;
using HiveSpace.Core.Contexts;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.OrderService.Domain.Aggregates.Carts;
using HiveSpace.OrderService.Domain.Exceptions;
using HiveSpace.OrderService.Domain.Repositories;

namespace HiveSpace.OrderService.Application.Cart.Commands.RemovePlatformCoupon;

public class RemovePlatformCouponCommandHandler(
    ICartRepository cartRepository,
    IUserContext userContext)
    : ICommandHandler<RemovePlatformCouponCommand>
{
    public async Task Handle(RemovePlatformCouponCommand request, CancellationToken cancellationToken)
    {
        var cart = await cartRepository.GetByUserIdAsync(userContext.UserId, cancellationToken)
            ?? throw new NotFoundException(OrderDomainErrorCode.CartNotFound, nameof(Cart));

        cart.RemovePlatformCoupon(request.CouponCode);
        await cartRepository.SaveChangesAsync(cancellationToken);
    }
}
