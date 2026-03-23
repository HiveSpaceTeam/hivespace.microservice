using HiveSpace.Core.Contexts;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.OrderService.Domain.Exceptions;
using HiveSpace.OrderService.Domain.Repositories;
using MediatR;


namespace HiveSpace.OrderService.Application.Orders.Commands.ConfirmPackage;

public class ConfirmPackageCommandHandler(
    IOrderRepository orderRepository,
    IUserContext userContext)
    : IRequestHandler<ConfirmPackageCommand, ConfirmPackageResult>
{
    public async Task<ConfirmPackageResult> Handle(ConfirmPackageCommand request, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetOrderByPackageIdAsync(request.PackageId, cancellationToken)
            ?? throw new NotFoundException(OrderDomainErrorCode.OrderPackageNotFound, request.PackageId.ToString());

        order.ConfirmPackage(request.PackageId, userContext.UserId);
        await orderRepository.SaveChangesAsync(cancellationToken);

        return new ConfirmPackageResult(order.Id, request.PackageId);
    }
}
