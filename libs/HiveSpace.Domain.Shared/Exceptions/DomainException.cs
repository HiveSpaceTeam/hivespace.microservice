using HiveSpace.Domain.Shared.Errors;

namespace HiveSpace.Domain.Shared.Exceptions;
public class DomainException(int httpCode, DomainErrorCode errorCode, string? source) : Exception
{
    private readonly int _httpCode = httpCode;
    public int HttpCode => _httpCode;

    private readonly DomainErrorCode _errorCode = errorCode;
    public DomainErrorCode ErrorCode => _errorCode;

    public override string? Source { get; set; } = source;
}
