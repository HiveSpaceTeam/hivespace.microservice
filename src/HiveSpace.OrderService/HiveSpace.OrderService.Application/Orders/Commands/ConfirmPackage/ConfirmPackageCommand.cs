using MediatR;

namespace HiveSpace.OrderService.Application.Orders.Commands.ConfirmPackage;

public record ConfirmPackageCommand(Guid PackageId) : IRequest<ConfirmPackageResult>;

public record ConfirmPackageResult(Guid CorrelationId, Guid OrderId, Guid PackageId);
