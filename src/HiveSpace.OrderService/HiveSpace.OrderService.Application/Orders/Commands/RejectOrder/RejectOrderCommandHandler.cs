using HiveSpace.Application.Shared.Handlers;
using HiveSpace.Core.Contexts;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.OrderService.Domain.Aggregates.Orders;
using HiveSpace.OrderService.Domain.Exceptions;
using HiveSpace.OrderService.Domain.Repositories;

namespace HiveSpace.OrderService.Application.Orders.Commands.RejectOrder;

public class RejectOrderCommandHandler(
    IOrderRepository orderRepository,
    IUserContext userContext)
    : ICommandHandler<RejectOrderCommand, RejectOrderResult>
{
    public async Task<RejectOrderResult> Handle(RejectOrderCommand request, CancellationToken cancellationToken)
    {
        if (userContext.StoreId is null)
            throw new ForbiddenException(OrderDomainErrorCode.SellerStoreRequired, nameof(userContext.StoreId));

        var order = await orderRepository.GetByIdAndStoreIdAsync(request.OrderId, userContext.StoreId.Value, cancellationToken)
            ?? throw new NotFoundException(OrderDomainErrorCode.OrderNotFound, nameof(Order));

        order.Reject(request.Reason, userContext.UserId);
        await orderRepository.SaveChangesAsync(cancellationToken);

        return new RejectOrderResult(order.Id, request.Reason, order.TotalAmount.Amount);
    }
}
