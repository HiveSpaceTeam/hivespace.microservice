using HiveSpace.OrderService.Application.Orders.Commands.ConfirmOrder;
using HiveSpace.OrderService.Application.Orders.Commands.RejectOrder;

namespace HiveSpace.OrderService.Application.Interfaces.Messaging;

public interface IOrderEventPublisher
{
    Task PublishOrderConfirmedBySellerAsync(ConfirmOrderResult result, CancellationToken cancellationToken = default);
    Task PublishOrderRejectedBySellerAsync(RejectOrderResult result, CancellationToken cancellationToken = default);
}
