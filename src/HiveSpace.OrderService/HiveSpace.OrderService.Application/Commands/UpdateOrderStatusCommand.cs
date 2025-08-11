using HiveSpace.OrderService.Domain.Enums;
using MediatR;

namespace HiveSpace.OrderService.Application.Commands;

public record UpdateOrderStatusCommand(
    Guid OrderId,
    OrderStatus Status
) : IRequest<bool>;