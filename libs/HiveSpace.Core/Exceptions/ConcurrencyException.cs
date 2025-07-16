using HiveSpace.Core.Exceptions.Models;

namespace HiveSpace.Core.Exceptions;
public class ConcurrencyException : ApplicationException
{
    private static readonly int _httpCode = 409; // HTTP 409 Conflict

    public ConcurrencyException(List<Error> errorCodeList, bool? enableData = false)
        : base(errorCodeList, _httpCode, enableData)
    {
    }

    public ConcurrencyException(List<Error> errorCodeList, Exception inner, bool? enableData = false)
        : base(errorCodeList, inner, _httpCode, enableData)
    {
    }
}
