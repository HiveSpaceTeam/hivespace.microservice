using HiveSpace.Domain.Shared.Entities;
using HiveSpace.Domain.Shared.Errors;

namespace HiveSpace.Core.Exceptions;

public class ApplicationErrorCode(int id, string name, string code) : Enumeration(id, name), IErrorCode
{
    public string Code { get; private set; } = code;
    public static readonly ApplicationErrorCode InternalServerError = new(0, "InternalServerError", "APP0000");
    public static readonly ApplicationErrorCode ConcurrencyException = new(1, "ConcurrencyException", "APP0001");
    public static readonly ApplicationErrorCode Forbidden = new(2, "Forbidden", "APP0002");
    public static readonly ApplicationErrorCode FluentValidationError = new(3, "FluentValidationError", "APP0003");
}