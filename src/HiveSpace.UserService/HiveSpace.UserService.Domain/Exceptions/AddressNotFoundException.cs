using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.UserService.Domain.Aggregates.User;

namespace HiveSpace.UserService.Domain.Exceptions;

public class AddressNotFoundException : DomainException
{
    public AddressNotFoundException()
        : base(404, UserDomainErrorCode.AddressNotFound, nameof(Address))
    {
    }
}
