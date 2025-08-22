using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.UserService.Domain.Aggregates.User;

namespace HiveSpace.UserService.Domain.Exceptions;

/// <summary>
/// Exception thrown when attempting to create a user that already exists.
/// </summary>
public class UserAlreadyExistsException : DomainException
{
    public UserAlreadyExistsException() 
        : base(409, UserDomainErrorCode.UserAlreadyExists, nameof(User))
    {
    }
}
