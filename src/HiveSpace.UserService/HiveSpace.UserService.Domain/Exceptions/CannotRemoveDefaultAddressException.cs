using HiveSpace.Domain.Shared.Exceptions;

namespace HiveSpace.UserService.Domain.Exceptions;

public class CannotRemoveDefaultAddressException : DomainException
{
    public CannotRemoveDefaultAddressException()
        : base(400, UserDomainErrorCode.CannotRemoveDefaultAddress, "Address")
    {
    }
}
