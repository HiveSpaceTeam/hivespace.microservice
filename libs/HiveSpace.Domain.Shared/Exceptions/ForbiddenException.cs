using HiveSpace.Domain.Shared.Errors;

namespace HiveSpace.Domain.Shared.Exceptions;

/// <summary>
/// Exception thrown when a user is not allowed to perform an action.
/// </summary>
public class ForbiddenException : DomainException
{
    private static readonly int _httpCode = 403;
    
    public ForbiddenException(DomainErrorCode errorCode, string source) 
        : base(_httpCode, errorCode, source)
    {
    }
}
