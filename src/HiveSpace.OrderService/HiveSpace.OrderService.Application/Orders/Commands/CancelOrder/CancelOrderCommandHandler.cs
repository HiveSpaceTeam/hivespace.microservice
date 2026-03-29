using HiveSpace.OrderService.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HiveSpace.OrderService.Application.Orders.Commands.CancelOrder;

public class CancelOrderCommandHandler(
    IOrderRepository orderRepository,
    ILogger<CancelOrderCommandHandler> logger)
    : IRequestHandler<CancelOrderCommand, CancelOrderResult>
{
    public async Task<CancelOrderResult> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByIdWithPackagesAsync(request.OrderId, cancellationToken);
        if (order is null)
        {
            logger.LogWarning("Order {OrderId} not found for cancellation — skipping", request.OrderId);
            return new CancelOrderResult(OrderFound: false);
        }

        order.Cancel(request.Reason, request.CancelledBy);
        await orderRepository.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Order {OrderId} cancelled: {Reason}", request.OrderId, request.Reason);
        return new CancelOrderResult(OrderFound: true);
    }
}
