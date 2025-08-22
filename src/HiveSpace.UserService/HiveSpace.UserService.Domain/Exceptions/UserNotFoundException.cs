using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.UserService.Domain.Aggregates.User;

namespace HiveSpace.UserService.Domain.Exceptions;

public class UserNotFoundException : DomainException
{
    public UserNotFoundException()
        : base(404, UserDomainErrorCode.UserNotFound, nameof(User))
    {
    }
}
