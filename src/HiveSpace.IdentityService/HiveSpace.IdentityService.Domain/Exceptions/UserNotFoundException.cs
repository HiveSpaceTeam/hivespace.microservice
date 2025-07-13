using HiveSpace.Domain.Shared;
using HiveSpace.IdentityService.Domain.Aggregates;

namespace HiveSpace.IdentityService.Domain.Exceptions;

public class UserNotFoundException : DomainException
{
    public UserNotFoundException()
        : base(404, IdentityErrorCode.UserNotFound, nameof(ApplicationUser))
    {
    }
}