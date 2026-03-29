using MediatR;

namespace HiveSpace.OrderService.Application.Orders.Commands.CancelOrder;

public record CancelOrderCommand(Guid OrderId, string Reason, Guid CancelledBy) : IRequest<CancelOrderResult>;

public record CancelOrderResult(bool OrderFound);
