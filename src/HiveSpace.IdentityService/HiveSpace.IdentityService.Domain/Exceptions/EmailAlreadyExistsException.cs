using HiveSpace.Domain.Shared;

namespace HiveSpace.IdentityService.Domain.Exceptions;

public class EmailAlreadyExistsException : DomainException
{
    public EmailAlreadyExistsException()
        : base(409, IdentityErrorCode.EmailAlreadyExists, nameof(EmailAlreadyExistsException))
    {
    }
}