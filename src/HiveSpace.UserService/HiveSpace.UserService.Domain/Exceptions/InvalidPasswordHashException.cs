using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.UserService.Domain.Aggregates.User;

namespace HiveSpace.UserService.Domain.Exceptions;

public class InvalidPasswordHashException : DomainException
{
    public InvalidPasswordHashException() : base(400, UserDomainErrorCode.InvalidPasswordHash, nameof(User.PasswordHash))
    {
    }
}
