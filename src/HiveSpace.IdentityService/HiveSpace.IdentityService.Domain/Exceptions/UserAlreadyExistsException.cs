using HiveSpace.Domain.Shared;

namespace HiveSpace.IdentityService.Domain.Exceptions;

public class UserAlreadyExistsException : DomainException
{
    public UserAlreadyExistsException()
        : base(409, IdentityErrorCode.UserAlreadyExists, nameof(UserAlreadyExistsException))
    {
    }
}