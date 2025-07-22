using HiveSpace.Domain.Shared.Errors;

namespace HiveSpace.IdentityService.Domain.Exceptions;
public class IdentityErrorCode(int id, string name, string code) : DomainErrorCode(id, name, code)
{
    public static readonly IdentityErrorCode UserNotFound = new(1, "UserNotFound", "IDN0001");
    public static readonly IdentityErrorCode UserAlreadyExists = new(2, "UserAlreadyExists", "IDN0002");
    public static readonly IdentityErrorCode InvalidPhoneNumber = new(3, "InvalidPhoneNumber", "IDN0003");
    public static readonly IdentityErrorCode EmailAlreadyExists = new(4, "EmailAlreadyExists", "IDN0004");
    public static readonly IdentityErrorCode AddressNotFound = new(5, "AddressNotFound", "IDN0005");
    public static readonly IdentityErrorCode InvalidUsername = new(6, "InvalidUsername", "IDN0006");
    public static readonly IdentityErrorCode InvalidEmail = new(7, "InvalidEmail", "IDN0007");
    public static readonly IdentityErrorCode InvalidFullName = new(8, "InvalidFullName", "IDN0008");
    public static readonly IdentityErrorCode InvalidPasswordFormat = new(9, "InvalidPasswordFormat", "IDN0009");
    public static readonly IdentityErrorCode PasswordMismatch = new(10, "PasswordMismatch", "IDN0010");
    public static readonly IdentityErrorCode InvalidDateOfBirth = new(11, "InvalidDateOfBirth", "IDN0011");
    public static readonly IdentityErrorCode Required = new(12, "Required", "IDN0012");
}
