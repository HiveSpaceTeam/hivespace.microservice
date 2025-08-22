using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.UserService.Domain.Aggregates.Store;

namespace HiveSpace.UserService.Domain.Exceptions;

/// <summary>
/// Exception thrown when a user already owns a store and tries to create another one.
/// </summary>
public class UserStoreExistsException : DomainException
{
    public UserStoreExistsException() 
        : base(409, UserDomainErrorCode.UserStoreExists, nameof(Store))
    {
    }
}
