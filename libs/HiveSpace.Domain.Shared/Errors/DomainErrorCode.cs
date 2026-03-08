using HiveSpace.Domain.Shared.Entities;

namespace HiveSpace.Domain.Shared.Errors;
public class DomainErrorCode(int id, string name, string code) : Enumeration(id, name), IErrorCode
{
    public static readonly DomainErrorCode ArgumentNull = new(1001, "ArgumentNull", "COM0001");
    public static readonly DomainErrorCode ParameterRequired = new(1002, "ParameterRequired", "COM0002");
    public static readonly DomainErrorCode InvalidEnumerationValue = new(1003, "InvalidEnumerationValue", "COM0003");
    public static readonly DomainErrorCode InvalidExpression = new(1004, "InvalidExpression", "COM0004");
    
    // Money Error Codes
    public static readonly DomainErrorCode MoneyNegativeAmount = new(1005, "MoneyNegativeAmount", "COM1005");
    public static readonly DomainErrorCode MoneyCurrencyMismatch = new(1006, "MoneyCurrencyMismatch", "COM1006");
    public static readonly DomainErrorCode MoneyInvalidFeeRate = new(1007, "MoneyInvalidFeeRate", "COM1007");
    public static readonly DomainErrorCode MoneyInvalidPercentage = new(1008, "MoneyInvalidPercentage", "COM1008");
    public static readonly DomainErrorCode MoneyDivideByZero = new(1009, "MoneyDivideByZero", "COM1009");
    public string Code { get; private set; } = code;
}
