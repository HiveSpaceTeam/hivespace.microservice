using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.UserService.Domain.Aggregates.Store;

namespace HiveSpace.UserService.Domain.Exceptions;

/// <summary>
/// Exception thrown when attempting to create a store that already exists.
/// </summary>
public class StoreAlreadyExistsException : DomainException
{
    public StoreAlreadyExistsException() 
        : base(409, UserDomainErrorCode.StoreNameAlreadyExists, nameof(Store))
    {
    }
}
