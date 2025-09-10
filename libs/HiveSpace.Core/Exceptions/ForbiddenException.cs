using HiveSpace.Core.Exceptions.Models;

namespace HiveSpace.Core.Exceptions;

public class ForbiddenException : HiveSpaceException
{
    private const int HttpStatusCode = 403;

    public ForbiddenException(IEnumerable<Error> errorList, bool? enableData = false)
        : base(errorList, HttpStatusCode, enableData)
    {
    }

    public ForbiddenException(IEnumerable<Error> errorList, Exception innerException, bool? enableData = false)
        : base(errorList, innerException, HttpStatusCode, enableData)
    {
    }
}
