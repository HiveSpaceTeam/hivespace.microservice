using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.UserService.Domain.Aggregates.User;

namespace HiveSpace.UserService.Domain.Exceptions;

/// <summary>
/// Exception thrown when user information provided is invalid.
/// </summary>
public class InvalidUserInformationException : DomainException
{
    public InvalidUserInformationException() 
        : base(400, UserDomainErrorCode.InvalidUserInformation, nameof(User))
    {
    }
}
