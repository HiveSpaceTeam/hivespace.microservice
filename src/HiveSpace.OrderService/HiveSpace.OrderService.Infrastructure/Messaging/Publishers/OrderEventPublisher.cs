using HiveSpace.Infrastructure.Messaging.Abstractions;
using HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Events;
using HiveSpace.OrderService.Application.Interfaces.Messaging;
using HiveSpace.OrderService.Application.Orders.Commands.ConfirmPackage;
using HiveSpace.OrderService.Application.Orders.Commands.RejectPackage;

namespace HiveSpace.OrderService.Infrastructure.Messaging.Publishers;

public class OrderEventPublisher(IMessageBus messageBus) : IOrderEventPublisher
{
    public Task PublishPackageConfirmedAsync(ConfirmPackageResult result, Guid storeId, CancellationToken cancellationToken = default)
        => messageBus.PublishAsync(new PackageConfirmed
        {
            CorrelationId = result.CorrelationId,
            OrderId       = result.OrderId,
            PackageId     = result.PackageId,
            StoreId       = storeId,
            ConfirmedAt   = DateTimeOffset.UtcNow
        }, cancellationToken);

    public Task PublishPackageRejectedAsync(RejectPackageResult result, CancellationToken cancellationToken = default)
        => messageBus.PublishAsync(new PackageRejected
        {
            CorrelationId = result.CorrelationId,
            OrderId       = result.OrderId,
            PackageId     = result.PackageId,
            Reason        = result.Reason,
            PackageAmount = result.PackageAmount
        }, cancellationToken);
}
