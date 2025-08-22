using HiveSpace.Domain.Shared.Exceptions;

namespace HiveSpace.UserService.Domain.Exceptions;

public class CannotRemoveOnlyAddressException : DomainException
{
    public CannotRemoveOnlyAddressException()
        : base(400, UserDomainErrorCode.CannotRemoveOnlyAddress, "Address")
    {
    }
}
