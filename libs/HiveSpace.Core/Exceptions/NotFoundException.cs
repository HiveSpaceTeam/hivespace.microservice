using HiveSpace.Core.Exceptions.Models;

namespace HiveSpace.Core.Exceptions;

public class NotFoundException : ApplicationException
{
    private static readonly int _httpCode = 404;

    public NotFoundException(List<ErrorCode> errorCodeList, bool? enableData = false) 
        : base(errorCodeList, _httpCode, enableData)
    {
    }

    public NotFoundException(List<ErrorCode> errorCodeList, Exception inner, bool? enableData = false) 
        : base(errorCodeList, inner, _httpCode, enableData)
    {
    }
}
