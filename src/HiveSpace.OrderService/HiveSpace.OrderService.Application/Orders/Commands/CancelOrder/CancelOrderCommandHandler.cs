using HiveSpace.Application.Shared.Handlers;
using HiveSpace.OrderService.Application.Orders;
using HiveSpace.OrderService.Domain.Enumerations;
using HiveSpace.OrderService.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace HiveSpace.OrderService.Application.Orders.Commands.CancelOrder;

public class CancelOrderCommandHandler(
    IOrderRepository orderRepository,
    ICouponRepository couponRepository,
    ILogger<CancelOrderCommandHandler> logger)
    : ICommandHandler<CancelOrderCommand, CancelOrderResult>
{
    public async Task<CancelOrderResult> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetDetailByIdAsync(request.OrderId, cancellationToken);
        if (order is null)
        {
            logger.LogWarning("Order {OrderId} not found for cancellation — skipping", request.OrderId);
            return new CancelOrderResult(OrderFound: false);
        }

        if (order.Status.Name == OrderStatus.Cancelled.Name)
        {
            logger.LogInformation("Order {OrderId} already cancelled — skipping", request.OrderId);
            return new CancelOrderResult(OrderFound: true);
        }

        var orderCouponUsages = order.Discounts
            .Select(discount => new OrderCouponUsageEntry(
                order.Id,
                order.UserId,
                discount.CouponCode,
                discount.DiscountAmount.Amount,
                discount.DiscountAmount.Currency))
            .ToList();

        order.Cancel(request.Reason, request.CancelledBy);
        await CheckoutCouponUsageRecorder.ReleaseAsync(orderCouponUsages, couponRepository, cancellationToken);
        await orderRepository.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Order {OrderId} cancelled: {Reason}", request.OrderId, request.Reason);
        return new CancelOrderResult(OrderFound: true);
    }
}
