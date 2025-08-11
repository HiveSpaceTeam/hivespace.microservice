using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.OrderService.Domain.AggregateRoots;

namespace HiveSpace.OrderService.Domain.Exceptions;

public class OrderNotFoundException : DomainException
{
    public OrderNotFoundException()
        : base(404, OrderErrorCode.OrderNotFound, nameof(Order))
    {
    }

    public OrderNotFoundException(Guid orderId)
        : base(404, OrderErrorCode.OrderNotFound, nameof(Order))
    {
    }
}