using HiveSpace.Domain.Shared;

namespace HiveSpace.IdentityService.Domain.Exceptions;

public class PasswordTooWeakException : DomainException
{
    public PasswordTooWeakException()
        : base(422, IdentityErrorCode.InvalidPasswordFormat, nameof(PasswordTooWeakException))
    {
    }
} 