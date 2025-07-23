using HiveSpace.Domain.Shared.Exceptions;

namespace HiveSpace.IdentityService.Domain.Exceptions;

public class PasswordTooWeakException : DomainException
{
    public PasswordTooWeakException()
        : base(422, IdentityErrorCode.InvalidPasswordFormat, nameof(PasswordTooWeakException))
    {
    }
} 