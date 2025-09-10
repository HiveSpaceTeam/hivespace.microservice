using HiveSpace.Core.Exceptions.Models;

namespace HiveSpace.Core.Exceptions;

public class BadRequestException : HiveSpaceException
{
    private const int HttpStatusCode = 400;

    public BadRequestException(IEnumerable<Error> errorList, bool? enableData = false) 
        : base(errorList, HttpStatusCode, enableData)
    {
    }

    public BadRequestException(IEnumerable<Error> errorList, Exception innerException, bool? enableData = false) 
        : base(errorList, innerException, HttpStatusCode, enableData)
    {
    }
}
