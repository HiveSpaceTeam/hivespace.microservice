using HiveSpace.Core.Exceptions.Models;

namespace HiveSpace.Core.Exceptions;

public class ForbiddenException : ApplicationException
{
    private static readonly int _httpCode = 403;

    public ForbiddenException(List<Error> errorCodeList, bool? enableData = false)
        : base(errorCodeList, _httpCode, enableData)
    {
    }

    public ForbiddenException(List<Error> errorCodeList, Exception inner, bool? enableData = false)
        : base(errorCodeList, inner, _httpCode, enableData)
    {
    }
}
