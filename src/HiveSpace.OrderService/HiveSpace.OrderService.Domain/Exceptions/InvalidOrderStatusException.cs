using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.OrderService.Domain.Enums;

namespace HiveSpace.OrderService.Domain.Exceptions;

public class InvalidOrderStatusException : DomainException
{
    public InvalidOrderStatusException()
        : base(400, OrderErrorCode.InvalidOrderStatus, nameof(InvalidOrderStatusException))
    {
    }

    public InvalidOrderStatusException(OrderStatus currentStatus, OrderStatus targetStatus)
        : base(400, OrderErrorCode.InvalidOrderStatus, nameof(InvalidOrderStatusException))
    {
    }
}