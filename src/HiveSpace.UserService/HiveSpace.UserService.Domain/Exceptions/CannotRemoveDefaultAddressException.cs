using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.UserService.Domain.Aggregates.User;

namespace HiveSpace.UserService.Domain.Exceptions;

public class CannotRemoveDefaultAddressException : DomainException
{
    public CannotRemoveDefaultAddressException()
        : base(400, UserDomainErrorCode.CannotRemoveDefaultAddress, nameof(Address))
    {
    }
}
