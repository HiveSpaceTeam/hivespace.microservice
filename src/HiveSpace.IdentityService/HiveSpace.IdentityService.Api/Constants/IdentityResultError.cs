namespace HiveSpace.IdentityService.Application.Constants;

/// <summary>
/// Contains string constants for Microsoft IdentityResult error codes.
/// </summary>
public static class IdentityResultError
{
    public const string PasswordTooShort = "PasswordTooShort";
    public const string PasswordRequiresNonAlphanumeric = "PasswordRequiresNonAlphanumeric";
    public const string PasswordRequiresDigit = "PasswordRequiresDigit";
    public const string PasswordRequiresLower = "PasswordRequiresLower";
    public const string PasswordRequiresUpper = "PasswordRequiresUpper";
    public const string DuplicateUserName = "DuplicateUserName";
    public const string DuplicateEmail = "DuplicateEmail";
    public const string InvalidUserName = "InvalidUserName";
    public const string InvalidEmail = "InvalidEmail";
    public const string InvalidToken = "InvalidToken";
    public const string LoginAlreadyAssociated = "LoginAlreadyAssociated";
    public const string PasswordMismatch = "PasswordMismatch";
    public const string UserAlreadyHasPassword = "UserAlreadyHasPassword";
    public const string UserAlreadyInRole = "UserAlreadyInRole";
    public const string UserNotInRole = "UserNotInRole";
    public const string UserLockoutNotEnabled = "UserLockoutNotEnabled";
    public const string UserAlreadyHasLogin = "UserAlreadyHasLogin";
    // Add more as needed from IdentityErrorDescriber
} 