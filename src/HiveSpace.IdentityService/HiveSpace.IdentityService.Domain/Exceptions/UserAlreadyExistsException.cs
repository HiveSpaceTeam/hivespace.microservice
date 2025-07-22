using HiveSpace.Domain.Shared.Exceptions;

namespace HiveSpace.IdentityService.Domain.Exceptions;

public class UserAlreadyExistsException : DomainException
{
    public UserAlreadyExistsException()
        : base(409, IdentityErrorCode.UserAlreadyExists, nameof(UserAlreadyExistsException))
    {
    }
}