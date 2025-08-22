using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.UserService.Domain.Aggregates.User;

namespace HiveSpace.UserService.Domain.Exceptions;

/// <summary>
/// Exception thrown when attempting to perform an operation with an inactive user.
/// </summary>
public class UserInactiveException : DomainException
{
    public UserInactiveException() 
        : base(400, UserDomainErrorCode.UserNotFound, nameof(User))
    {
    }
}
