using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.IdentityService.Domain.Aggregates;

namespace HiveSpace.IdentityService.Domain.Exceptions;

public class UserNotFoundException : DomainException
{
    public UserNotFoundException()
        : base(404, IdentityErrorCode.UserNotFound, nameof(ApplicationUser))
    {
    }
}