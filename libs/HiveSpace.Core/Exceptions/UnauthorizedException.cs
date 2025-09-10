using HiveSpace.Core.Exceptions.Models;

namespace HiveSpace.Core.Exceptions;

public class UnauthorizedException : HiveSpaceException
{
    private const int HttpStatusCode = 401;

    public UnauthorizedException(IEnumerable<Error> errorList, bool? enableData = false)
        : base(errorList, HttpStatusCode, enableData)
    {
    }

    public UnauthorizedException(IEnumerable<Error> errorList, Exception innerException, bool? enableData = false)
        : base(errorList, innerException, HttpStatusCode, enableData)
    {
    }
}
