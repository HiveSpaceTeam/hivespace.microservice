using HiveSpace.Core.Exceptions;
using HiveSpace.Core.Exceptions.Models;
using HiveSpace.OrderService.Domain.Exceptions;

namespace HiveSpace.OrderService.Application.Exceptions;

public class OrderNotFoundApplicationException : NotFoundException
{
    public OrderNotFoundApplicationException(Guid orderId, bool? enableData = false)
        : base([new Error(OrderErrorCode.OrderNotFound, $"Order with ID {orderId}")], enableData)
    {
    }

    public OrderNotFoundApplicationException(Guid orderId, Exception inner, bool? enableData = false)
        : base([new Error(OrderErrorCode.OrderNotFound, $"Order with ID {orderId}")], inner, enableData)
    {
    }
}