using HiveSpace.Application.Shared.Commands;

namespace HiveSpace.OrderService.Application.Orders.Commands.CancelOrder;

public record CancelOrderCommand(Guid OrderId, string Reason, Guid CancelledBy) : ICommand<CancelOrderResult>;

public record CancelOrderResult(bool OrderFound);
