using System.Net;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.UserService.Domain.Aggregates.Admin;

namespace HiveSpace.UserService.Domain.Exceptions;

public class AdminNotFoundException : DomainException
{
    public AdminNotFoundException() 
        : base(404, UserDomainErrorCode.AdminNotFound, nameof(Admin))
    {
    }
}
