using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.UserService.Domain.Aggregates.User;

namespace HiveSpace.UserService.Domain.Exceptions;

/// <summary>
/// Exception thrown when a username is already taken.
/// </summary>
public class UserNameAlreadyExistsException : DomainException
{
    public UserNameAlreadyExistsException() 
        : base(409, UserDomainErrorCode.UserNameAlreadyExists, nameof(User))
    {
    }
}
