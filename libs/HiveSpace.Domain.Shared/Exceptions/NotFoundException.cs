using HiveSpace.Domain.Shared.Errors;

namespace HiveSpace.Domain.Shared.Exceptions;

public class NotFoundException(DomainErrorCode errorCode, string source) : DomainException(_httpCode, errorCode, source)
{
    private static readonly int _httpCode = 404;
}
