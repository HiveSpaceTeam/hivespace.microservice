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
    public static readonly IdentityDomainErrorCode InvalidCredentials = new(6006, "InvalidCredentials", "IDN6006");
    public static readonly IdentityDomainErrorCode AccountInactive = new(6007, "AccountInactive", "IDN6007");
    public static readonly IdentityDomainErrorCode AccountLocked = new(6008, "AccountLocked", "IDN6008");
    public static readonly IdentityDomainErrorCode AccountNotAllowed = new(6009, "AccountNotAllowed", "IDN6009");
    public static readonly IdentityDomainErrorCode DuplicateEmail = new(6010, "DuplicateEmail", "IDN6010");
    public static readonly IdentityDomainErrorCode InvalidSession = new(6011, "InvalidSession", "IDN6011");
    public static readonly IdentityDomainErrorCode SessionExpired = new(6012, "SessionExpired", "IDN6012");
    public static readonly IdentityDomainErrorCode InvalidReturnUrl = new(6013, "InvalidReturnUrl", "IDN6013");
}
