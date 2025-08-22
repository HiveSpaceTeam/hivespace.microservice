using HiveSpace.Domain.Shared.Errors;

namespace HiveSpace.UserService.Domain.Exceptions;

public class UserDomainErrorCode : DomainErrorCode
{
    private UserDomainErrorCode(int id, string name, string code) : base(id, name, code) { }

    // User-related errors
    public static readonly UserDomainErrorCode UserNotFound = new(1, "UserNotFound", "USR0001");
    public static readonly UserDomainErrorCode UserAlreadyExists = new(2, "UserAlreadyExists", "USR0002");
    public static readonly UserDomainErrorCode InvalidUserRole = new(3, "InvalidUserRole", "USR0003");
    public static readonly UserDomainErrorCode InvalidUserInformation = new(21, "InvalidUserInformation", "USR0021");
    public static readonly UserDomainErrorCode UserNameAlreadyExists = new(22, "UserNameAlreadyExists", "USR0022");
    public static readonly UserDomainErrorCode InvalidUserId = new(20, "InvalidUserId", "USR0020");
    
    // Profile-related errors
    public static readonly UserDomainErrorCode InvalidEmail = new(4, "InvalidEmail", "USR0004");
    public static readonly UserDomainErrorCode EmailAlreadyExists = new(5, "EmailAlreadyExists", "USR0005");
    public static readonly UserDomainErrorCode InvalidPhoneNumber = new(6, "InvalidPhoneNumber", "USR0006");
    public static readonly UserDomainErrorCode InvalidDateOfBirth = new(7, "InvalidDateOfBirth", "USR0007");
    public static readonly UserDomainErrorCode InvalidPasswordHash = new(8, "InvalidPasswordHash", "USR0008");
    
    // Address-related errors
    public static readonly UserDomainErrorCode AddressNotFound = new(9, "AddressNotFound", "USR0009");
    public static readonly UserDomainErrorCode CannotRemoveDefaultAddress = new(10, "CannotRemoveDefaultAddress", "USR0010");
    public static readonly UserDomainErrorCode CannotRemoveOnlyAddress = new(11, "CannotRemoveOnlyAddress", "USR0011");
    
    // Store-related errors
    public static readonly UserDomainErrorCode StoreNameAlreadyExists = new(12, "StoreNameAlreadyExists", "USR0012");
    public static readonly UserDomainErrorCode StoreNotFound = new(13, "StoreNotFound", "USR0013");
    public static readonly UserDomainErrorCode InvalidStoreInformation = new(18, "InvalidStoreInformation", "USR0018");
    public static readonly UserDomainErrorCode UserStoreExists = new(19, "UserStoreExists", "USR0019");
    
    // Admin-related errors
    public static readonly UserDomainErrorCode AdminNotFound = new(14, "AdminNotFound", "USR0014");
    public static readonly UserDomainErrorCode AdminAlreadyExists = new(15, "AdminAlreadyExists", "USR0015");
    public static readonly UserDomainErrorCode AdminInactive = new(16, "AdminInactive", "USR0016");
    public static readonly UserDomainErrorCode CannotModifySystemAdmin = new(17, "CannotModifySystemAdmin", "USR0017");
}
