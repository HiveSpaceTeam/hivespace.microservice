using HiveSpace.Application.Shared.Handlers;
using HiveSpace.Core.Contexts;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.OrderService.Domain.Aggregates.Orders;
using HiveSpace.OrderService.Domain.Exceptions;
using HiveSpace.OrderService.Domain.Repositories;

namespace HiveSpace.OrderService.Application.Orders.Commands.ConfirmOrder;

public class ConfirmOrderCommandHandler(
    IOrderRepository orderRepository,
    IUserContext userContext)
    : ICommandHandler<ConfirmOrderCommand, ConfirmOrderResult>
{
    public async Task<ConfirmOrderResult> Handle(ConfirmOrderCommand request, CancellationToken cancellationToken)
    {
        if (userContext.StoreId is null)
            throw new ForbiddenException(OrderDomainErrorCode.SellerStoreRequired, nameof(userContext.StoreId));

        var order = await orderRepository.GetByIdAndStoreIdAsync(request.OrderId, userContext.StoreId.Value, cancellationToken)
            ?? throw new NotFoundException(OrderDomainErrorCode.OrderNotFound, nameof(Order));

        order.Confirm(userContext.UserId);
        await orderRepository.SaveChangesAsync(cancellationToken);

        return new ConfirmOrderResult(order.Id, order.StoreId);
    }
}
