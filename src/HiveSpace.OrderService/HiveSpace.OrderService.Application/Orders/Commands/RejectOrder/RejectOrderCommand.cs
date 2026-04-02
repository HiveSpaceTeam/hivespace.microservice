using HiveSpace.Application.Shared.Commands;

namespace HiveSpace.OrderService.Application.Orders.Commands.RejectOrder;

public record RejectOrderCommand(Guid OrderId, string Reason) : ICommand<RejectOrderResult>;

public record RejectOrderResult(Guid OrderId, string Reason, long OrderAmount);
