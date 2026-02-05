using HiveSpace.Domain.Shared.Entities;

namespace HiveSpace.Domain.Shared.Errors;
public class DomainErrorCode(int id, string name, string code) : Enumeration(id, name), IErrorCode
{
    public static readonly DomainErrorCode ArgumentNull = new(1001, "ArgumentNull", "COM0001");
    public static readonly DomainErrorCode ParameterRequired = new(1002, "ParameterRequired", "COM0002");
    public static readonly DomainErrorCode InvalidEnumerationValue = new(1003, "InvalidEnumerationValue", "COM0003");
    public static readonly DomainErrorCode InvalidExpression = new(1004, "InvalidExpression", "COM0004");
    public string Code { get; private set; } = code;
}
