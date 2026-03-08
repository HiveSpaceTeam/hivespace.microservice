using HiveSpace.Domain.Shared.Entities;
using HiveSpace.Domain.Shared.Errors;

namespace HiveSpace.Core.Exceptions;

public class CommonErrorCode(int id, string name, string code) : Enumeration(id, name), IErrorCode
{
    public string Code { get; private set; } = code;
    public static readonly CommonErrorCode InternalServerError = new(0, "InternalServerError", "APP0000");
    public static readonly CommonErrorCode ConcurrencyException = new(1, "ConcurrencyException", "APP0001");
    public static readonly CommonErrorCode Forbidden = new(2, "Forbidden", "APP0002");
    public static readonly CommonErrorCode FluentValidationError = new(3, "FluentValidationError", "APP0003");
    public static readonly CommonErrorCode Required = new(4, "Required", "APP0004");
    public static readonly CommonErrorCode InvalidOperation = new(5, "InvalidOperation", "APP0005");
    public static readonly CommonErrorCode ArgumentNull = new(6, "ArgumentNull", "APP0006");
    public static readonly CommonErrorCode InvalidArgument = new(7, "InvalidArgument", "APP0007");
    public static readonly CommonErrorCode ConfigurationMissing = new(10, "ConfigurationMissing", "APP0008");
    public static readonly CommonErrorCode RoleMappingError = new(11, "RoleMappingError", "APP0009");
    public static readonly CommonErrorCode InvalidPageNumber = new(12, "InvalidPageNumber", "APP0010");
    public static readonly CommonErrorCode InvalidPageSize = new(13, "InvalidPageSize", "APP0011");
    public static readonly CommonErrorCode InvalidRoleFilter = new(14, "InvalidRoleFilter", "APP0012");
    public static readonly CommonErrorCode InvalidStatusFilter = new(15, "InvalidStatusFilter", "APP0013");
    public static readonly CommonErrorCode InvalidSortFormat = new(17, "InvalidSortFormat", "APP0015");
    public static readonly CommonErrorCode SubClaimMissing = new(18, "SubClaimMissing", "APP0016");
    public static readonly CommonErrorCode SubClaimInvalid = new(19, "SubClaimInvalid", "APP0017");
}