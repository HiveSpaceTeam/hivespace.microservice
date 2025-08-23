using HiveSpace.Domain.Shared.Errors;

namespace HiveSpace.Domain.Shared.Exceptions;

public class InvalidFieldException : DomainException
{
    private static readonly int _httpCode = 400;
    
    public InvalidFieldException(DomainErrorCode errorCode, string source) 
        : base(_httpCode, errorCode, source)
    {
    }
}
