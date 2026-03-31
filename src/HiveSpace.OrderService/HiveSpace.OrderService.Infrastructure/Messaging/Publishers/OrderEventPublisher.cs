using HiveSpace.Infrastructure.Messaging.Abstractions;
using HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Events;
using HiveSpace.OrderService.Application.Interfaces.Messaging;
using HiveSpace.OrderService.Application.Orders.Commands.ConfirmOrder;
using HiveSpace.OrderService.Application.Orders.Commands.RejectOrder;

namespace HiveSpace.OrderService.Infrastructure.Messaging.Publishers;

public class OrderEventPublisher(IMessageBus messageBus) : IOrderEventPublisher
{
    public Task PublishOrderConfirmedBySellerAsync(ConfirmOrderResult result, CancellationToken cancellationToken = default)
        => messageBus.PublishAsync(new OrderConfirmedBySeller
        {
            CorrelationId = result.OrderId,
            OrderId       = result.OrderId,
            StoreId       = result.StoreId,
            ConfirmedAt   = DateTimeOffset.UtcNow
        }, cancellationToken);

    public Task PublishOrderRejectedBySellerAsync(RejectOrderResult result, CancellationToken cancellationToken = default)
        => messageBus.PublishAsync(new OrderRejectedBySeller
        {
            CorrelationId = result.OrderId,
            OrderId       = result.OrderId,
            Reason        = result.Reason,
            OrderAmount   = result.OrderAmount
        }, cancellationToken);
}
