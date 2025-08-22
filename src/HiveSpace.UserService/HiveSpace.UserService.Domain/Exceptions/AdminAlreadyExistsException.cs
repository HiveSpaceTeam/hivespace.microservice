using System.Net;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.UserService.Domain.Aggregates.Admin;

namespace HiveSpace.UserService.Domain.Exceptions;

public class AdminAlreadyExistsException : DomainException
{
    public AdminAlreadyExistsException() 
        : base(409, UserDomainErrorCode.AdminAlreadyExists, nameof(Admin))
    {
    }
}
