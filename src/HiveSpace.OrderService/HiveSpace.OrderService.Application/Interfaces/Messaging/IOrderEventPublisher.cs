using HiveSpace.OrderService.Application.Orders.Commands.ConfirmPackage;
using HiveSpace.OrderService.Application.Orders.Commands.RejectPackage;

namespace HiveSpace.OrderService.Application.Interfaces.Messaging;

public interface IOrderEventPublisher
{
    Task PublishPackageConfirmedAsync(ConfirmPackageResult result, Guid storeId, CancellationToken cancellationToken = default);
    Task PublishPackageRejectedAsync(RejectPackageResult result, CancellationToken cancellationToken = default);
}
