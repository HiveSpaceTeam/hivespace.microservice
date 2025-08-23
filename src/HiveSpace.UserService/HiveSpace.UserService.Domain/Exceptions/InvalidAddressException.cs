using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.UserService.Domain.Aggregates.User;

namespace HiveSpace.UserService.Domain.Exceptions;

/// <summary>
/// Exception thrown when address information provided is invalid.
/// </summary>
public class InvalidAddressException : DomainException
{
    public InvalidAddressException(string field) 
        : base(400, UserDomainErrorCode.InvalidAddress, field)
    {
    }
}
