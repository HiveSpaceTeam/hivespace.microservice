using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.UserService.Domain.Aggregates.User;

namespace HiveSpace.UserService.Domain.Exceptions;

public class InvalidPasswordException : DomainException
{
    public InvalidPasswordException() : base(400, UserDomainErrorCode.InvalidPassword, nameof(User.PasswordHash))
    {
    }
}
