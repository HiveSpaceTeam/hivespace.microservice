using HiveSpace.Domain.Shared.Entities;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.OrderService.Domain.Exceptions;
using System.Text.RegularExpressions;

namespace HiveSpace.OrderService.Domain.ValueObjects;

public partial class PhoneNumber : ValueObject
{
    public string Value { get; private set; }
    public Regex regex = PhoneNumberRegex();

    private PhoneNumber() { Value = string.Empty; }

    public PhoneNumber(string value)
    {
        Value = value;
        ValidateValueObject();
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    protected void ValidateValueObject()
    {
        if (string.IsNullOrWhiteSpace(Value))
        {
            return;
        }
        if (!regex.IsMatch(Value))
        {
            throw new DomainException(400, OrderErrorCode.InvalidPhoneNumber, nameof(PhoneNumber));
        }
    }

    [GeneratedRegex(@"^84(?:3[2-9]|5[6|8|9]|7[0|6-9]|8[1-9]|9[0-9])\d{7}$")]
    private static partial Regex PhoneNumberRegex();
}