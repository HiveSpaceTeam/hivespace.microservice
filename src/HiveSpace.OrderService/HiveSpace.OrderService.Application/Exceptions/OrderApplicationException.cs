using HiveSpace.Core.Exceptions.Models;

namespace HiveSpace.OrderService.Application.Exceptions;

public class OrderApplicationException : Core.Exceptions.ApplicationException
{
    public OrderApplicationException(List<Error> errorCodeList, bool? enableData = false)
        : base(errorCodeList, null, enableData)
    {
    }

    public OrderApplicationException(List<Error> errorCodeList, Exception inner, bool? enableData = false)
        : base(errorCodeList, inner, null, enableData)
    {
    }
}