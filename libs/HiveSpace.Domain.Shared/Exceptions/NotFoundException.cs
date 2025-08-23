using HiveSpace.Domain.Shared.Errors;

namespace HiveSpace.Domain.Shared.Exceptions;

public class NotFoundException : DomainException
{
    private static readonly int _httpCode = 404;
    
    public NotFoundException(DomainErrorCode errorCode, string source) 
        : base(_httpCode, errorCode, source)
    {
    }
}
