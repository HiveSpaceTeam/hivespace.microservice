using System.Net;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.UserService.Domain.Aggregates.Admin;

namespace HiveSpace.UserService.Domain.Exceptions;

public class AdminInactiveException : DomainException
{
    public AdminInactiveException() 
        : base(403, UserDomainErrorCode.AdminInactive, nameof(Admin))
    {
    }
}
