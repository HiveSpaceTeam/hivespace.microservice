using HiveSpace.Domain.Shared.Exceptions;

namespace HiveSpace.IdentityService.Domain.Exceptions;

public class InvalidPasswordException : DomainException
{
    public InvalidPasswordException()
        : base(422, IdentityErrorCode.InvalidPasswordFormat, nameof(InvalidPasswordException))
    {
    }
} 