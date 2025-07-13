using HiveSpace.Domain.Shared;
using HiveSpace.IdentityService.Domain.Aggregates;

namespace HiveSpace.IdentityService.Domain.Exceptions;

public class AddressNotFoundException : DomainException
{
    public AddressNotFoundException()
        : base(404, IdentityErrorCode.AddressNotFound, nameof(Address))
    {
    }
}   