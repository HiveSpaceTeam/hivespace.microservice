using MediatR;

namespace HiveSpace.OrderService.Application.Orders.Commands.RejectPackage;

public record RejectPackageCommand(Guid PackageId, string Reason) : IRequest<RejectPackageResult>;

public record RejectPackageResult(Guid CorrelationId, Guid OrderId, Guid PackageId, string Reason, decimal PackageAmount);
