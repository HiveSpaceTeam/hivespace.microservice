using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.UserService.Domain.Aggregates.User;

namespace HiveSpace.UserService.Domain.Exceptions;

public class InvalidDateOfBirthException : DomainException
{
    public InvalidDateOfBirthException() : base(400, UserDomainErrorCode.InvalidDateOfBirth, nameof(DateOfBirth))
    {
    }
}