using HiveSpace.Application.Shared.Commands;

namespace HiveSpace.OrderService.Application.Orders.Commands.ConfirmOrder;

public record ConfirmOrderCommand(Guid OrderId) : ICommand<ConfirmOrderResult>;

public record ConfirmOrderResult(Guid OrderId, Guid StoreId);
