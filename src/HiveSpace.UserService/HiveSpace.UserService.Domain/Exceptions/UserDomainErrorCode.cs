using HiveSpace.Domain.Shared.Errors;

namespace HiveSpace.UserService.Domain.Exceptions;

public class UserDomainErrorCode : DomainErrorCode
{
    private UserDomainErrorCode(int id, string name, string code) : base(id, name, code) { }

    // User-related errors (ID: 1-3)
    public static readonly UserDomainErrorCode UserNotFound = new(1, "UserNotFound", "USR0001");
    public static readonly UserDomainErrorCode UserAlreadyExists = new(2, "UserAlreadyExists", "USR0002");
    public static readonly UserDomainErrorCode UserInactive = new(3, "UserInactive", "USR0003");
    
    // Profile-related errors (ID: 4-7)
    public static readonly UserDomainErrorCode InvalidEmail = new(4, "InvalidEmail", "USR0004");
    public static readonly UserDomainErrorCode EmailAlreadyExists = new(5, "EmailAlreadyExists", "USR0005");
    public static readonly UserDomainErrorCode InvalidPhoneNumber = new(6, "InvalidPhoneNumber", "USR0006");
    public static readonly UserDomainErrorCode InvalidDateOfBirth = new(7, "InvalidDateOfBirth", "USR0007");
    public static readonly UserDomainErrorCode InvalidPasswordHash = new(8, "InvalidPasswordHash", "USR0008");
    
    // Address-related errors (ID: 9-11)
    public static readonly UserDomainErrorCode AddressNotFound = new(9, "AddressNotFound", "USR0009");
    public static readonly UserDomainErrorCode CannotRemoveDefaultAddress = new(10, "CannotRemoveDefaultAddress", "USR0010");
    public static readonly UserDomainErrorCode CannotRemoveOnlyAddress = new(11, "CannotRemoveOnlyAddress", "USR0011");
    
    // Store-related errors (ID: 12-13, 18-19)
    public static readonly UserDomainErrorCode StoreNameAlreadyExists = new(12, "StoreNameAlreadyExists", "USR0012");
    public static readonly UserDomainErrorCode StoreNotFound = new(13, "StoreNotFound", "USR0013");
    
    // Store-related errors (continued)
    public static readonly UserDomainErrorCode InvalidStoreInformation = new(18, "InvalidStoreInformation", "USR0018");
    public static readonly UserDomainErrorCode UserStoreExists = new(19, "UserStoreExists", "USR0019");
    
    // User-related errors (continued)
    public static readonly UserDomainErrorCode InvalidUserInformation = new(20, "InvalidUserInformation", "USR0021");
    public static readonly UserDomainErrorCode UserNameAlreadyExists = new(21, "UserNameAlreadyExists", "USR0022");
    
    // Field validation errors (ID: 22-23)
    public static readonly UserDomainErrorCode InvalidField = new(22, "InvalidField", "USR0023");
    public static readonly UserDomainErrorCode InvalidAddress = new(23, "InvalidAddress", "USR0024");
    
    // Authorization errors (ID: 24)
    public static readonly UserDomainErrorCode InsufficientPrivileges = new(24, "InsufficientPrivileges", "USR0025");
}
