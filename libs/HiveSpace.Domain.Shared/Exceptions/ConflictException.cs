using HiveSpace.Domain.Shared.Errors;

namespace HiveSpace.Domain.Shared.Exceptions;

/// <summary>
/// Exception thrown when a resource conflict occurs (e.g., duplicate entries, concurrent modifications).
/// </summary>
public class ConflictException(DomainErrorCode errorCode, string source) : DomainException(_httpCode, errorCode, source)
{
    private static readonly int _httpCode = 409;
}
