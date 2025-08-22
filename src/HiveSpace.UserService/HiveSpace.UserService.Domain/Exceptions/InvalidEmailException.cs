using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.UserService.Domain.Aggregates.User;

namespace HiveSpace.UserService.Domain.Exceptions;

public class InvalidEmailException : DomainException
{
    public InvalidEmailException(string? message = null) : base(400, UserDomainErrorCode.InvalidEmail, nameof(Email))
    {
    }
}
