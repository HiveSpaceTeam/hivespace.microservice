using HiveSpace.Domain.Shared.Exceptions;

namespace HiveSpace.UserService.Domain.Exceptions;

public class InvalidPasswordHashException : DomainException
{
    public InvalidPasswordHashException() : base(400, UserDomainErrorCode.InvalidPasswordHash, "PasswordHash")
    {
    }
}
