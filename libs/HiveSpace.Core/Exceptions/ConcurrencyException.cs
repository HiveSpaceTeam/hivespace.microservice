using HiveSpace.Core.Exceptions.Models;

namespace HiveSpace.Core.Exceptions;

public class ConcurrencyException : HiveSpaceException
{
    private const int HttpStatusCode = 409; // HTTP 409 Conflict

    public ConcurrencyException(IEnumerable<Error> errorList, bool? enableData = false)
        : base(errorList, HttpStatusCode, enableData)
    {
    }

    public ConcurrencyException(IEnumerable<Error> errorList, Exception innerException, bool? enableData = false)
        : base(errorList, innerException, HttpStatusCode, enableData)
    {
    }
}
