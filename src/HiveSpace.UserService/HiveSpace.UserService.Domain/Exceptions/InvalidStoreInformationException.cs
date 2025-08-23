using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.UserService.Domain.Aggregates.Store;

namespace HiveSpace.UserService.Domain.Exceptions;

/// <summary>
/// Exception thrown when store information provided is invalid.
/// </summary>
public class InvalidStoreInformationException : DomainException
{
    public InvalidStoreInformationException() 
        : base(400, UserDomainErrorCode.InvalidStoreInformation, nameof(Store))
    {
    }
}
