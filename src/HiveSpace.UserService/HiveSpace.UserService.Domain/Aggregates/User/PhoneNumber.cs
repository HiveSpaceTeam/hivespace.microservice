using HiveSpace.Domain.Shared.Entities;
using HiveSpace.UserService.Domain.Exceptions;

namespace HiveSpace.UserService.Domain.Aggregates.User;

public class PhoneNumber : ValueObject
{
    public string Value { get; }
    
    private PhoneNumber() 
    {
        Value = string.Empty; // For EF Core
    }
    
    public PhoneNumber(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidPhoneNumberException("Phone number cannot be empty");
            
        if (!IsValidPhoneNumber(value))
            throw new InvalidPhoneNumberException("Invalid phone number format");
            
        Value = value.Trim();
    }
    
    private static bool IsValidPhoneNumber(string phoneNumber)
    {
        // Basic validation - can be enhanced based on requirements
        var cleanNumber = new string(phoneNumber.Where(char.IsDigit).ToArray());
        return cleanNumber.Length >= 10 && cleanNumber.Length <= 15;
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
    
    public static implicit operator string(PhoneNumber phoneNumber) => phoneNumber.Value;
    public static explicit operator PhoneNumber(string value) => new PhoneNumber(value);
    
    // Factory methods for better creation control
    public static PhoneNumber Create(string value) => new PhoneNumber(value);
    public static PhoneNumber? CreateOrDefault(string? value) => 
        string.IsNullOrWhiteSpace(value) ? null : new PhoneNumber(value);
}
