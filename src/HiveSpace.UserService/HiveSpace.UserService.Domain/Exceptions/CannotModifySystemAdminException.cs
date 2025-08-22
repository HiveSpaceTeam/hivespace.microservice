using System.Net;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.UserService.Domain.Aggregates.Admin;

namespace HiveSpace.UserService.Domain.Exceptions;

public class CannotModifySystemAdminException : DomainException
{
    public CannotModifySystemAdminException() 
        : base(403, UserDomainErrorCode.CannotModifySystemAdmin, nameof(Admin))
    {
    }
}
