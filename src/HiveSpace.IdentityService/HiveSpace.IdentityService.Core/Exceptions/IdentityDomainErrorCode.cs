using HiveSpace.Domain.Shared.Errors;

namespace HiveSpace.IdentityService.Core.Exceptions;

public class IdentityDomainErrorCode : DomainErrorCode
{
    private IdentityDomainErrorCode(int id, string name, string code) : base(id, name, code) { }

    public static readonly IdentityDomainErrorCode IdentityUserNotFound = new(6001, "IdentityUserNotFound", "IDN6001");
    public static readonly IdentityDomainErrorCode InvalidConfiguration = new(6002, "InvalidConfiguration", "IDN6002");
    public static readonly IdentityDomainErrorCode EmailAlreadyVerified = new(6003, "EmailAlreadyVerified", "IDN6003");
    public static readonly IdentityDomainErrorCode EmailVerificationFailed = new(6004, "EmailVerificationFailed", "IDN6004");
    public static readonly IdentityDomainErrorCode IdentityUserCreationFailed = new(6005, "IdentityUserCreationFailed", "IDN6005");
}
