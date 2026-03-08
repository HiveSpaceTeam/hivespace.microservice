using HiveSpace.Domain.Shared.Errors;

namespace HiveSpace.Domain.Shared.Exceptions;

/// <summary>
/// Exception thrown when a user is not allowed to perform an action.
/// </summary>
public class ForbiddenException(DomainErrorCode errorCode, string source) : DomainException(_httpCode, errorCode, source)
{
    private static readonly int _httpCode = 403;
}
