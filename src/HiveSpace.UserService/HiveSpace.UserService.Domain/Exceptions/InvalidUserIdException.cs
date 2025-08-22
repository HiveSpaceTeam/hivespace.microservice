using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.UserService.Domain.Aggregates.User;

namespace HiveSpace.UserService.Domain.Exceptions;

/// <summary>
/// Exception thrown when a user ID is invalid or empty.
/// </summary>
public class InvalidUserIdException : DomainException
{
    public InvalidUserIdException() 
        : base(400, UserDomainErrorCode.InvalidUserId, nameof(User))
    {
    }
}
