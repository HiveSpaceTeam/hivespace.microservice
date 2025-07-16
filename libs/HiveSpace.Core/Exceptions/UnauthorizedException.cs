using HiveSpace.Core.Exceptions.Models;

namespace HiveSpace.Core.Exceptions;
public class UnauthorizedException : ApplicationException
{
    private static readonly int _httpCode = 401;

    public UnauthorizedException(List<Error> errorCodeList, bool? enableData = false)
        : base(errorCodeList, _httpCode, enableData)
    {
    }

    public UnauthorizedException(List<Error> errorCodeList, Exception inner, bool? enableData = false)
        : base(errorCodeList, inner, _httpCode, enableData)
    {
    }
}
