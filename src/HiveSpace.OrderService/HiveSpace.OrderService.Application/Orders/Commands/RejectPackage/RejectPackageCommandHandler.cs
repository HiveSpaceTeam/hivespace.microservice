using HiveSpace.Core.Contexts;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.OrderService.Domain.Exceptions;
using HiveSpace.OrderService.Domain.Repositories;
using MediatR;

namespace HiveSpace.OrderService.Application.Orders.Commands.RejectPackage;

public class RejectPackageCommandHandler(
    IOrderRepository orderRepository,
    IUserContext userContext)
    : IRequestHandler<RejectPackageCommand, RejectPackageResult>
{
    public async Task<RejectPackageResult> Handle(RejectPackageCommand request, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetOrderByPackageIdAsync(request.PackageId, cancellationToken)
            ?? throw new NotFoundException(OrderDomainErrorCode.OrderPackageNotFound, nameof(request.PackageId));

        var package = order.Packages.FirstOrDefault(p => p.Id == request.PackageId)
            ?? throw new NotFoundException(OrderDomainErrorCode.OrderPackageNotFound, nameof(request.PackageId));

        var packageAmount = (decimal)package.TotalAmount.Amount;

        order.RejectPackage(request.PackageId, request.Reason, userContext.UserId);
        await orderRepository.SaveChangesAsync(cancellationToken);

        return new RejectPackageResult(order.Id, order.Id, request.PackageId, request.Reason, packageAmount);
    }
}
