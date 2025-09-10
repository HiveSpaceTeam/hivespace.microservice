using HiveSpace.Core.Exceptions.Models;

namespace HiveSpace.Core.Exceptions;

/// <summary>
/// Exception thrown when a validation fails.
/// Uses error codes only - no hardcoded messages for dual-language support.
/// </summary>
public class ValidationException : HiveSpaceException
{
    private const int HttpStatusCode = 422; // Unprocessable Entity

    public ValidationException(IEnumerable<Error> errorList, bool? enableData = false) 
        : base(errorList, HttpStatusCode, enableData)
    {
    }

    public ValidationException(IEnumerable<Error> errorList, Exception innerException, bool? enableData = false) 
        : base(errorList, innerException, HttpStatusCode, enableData)
    {
    }
}
