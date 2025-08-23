using HiveSpace.Domain.Shared.Entities;
using HiveSpace.UserService.Domain.Exceptions;

namespace HiveSpace.UserService.Domain.Aggregates.User;

public class PhoneNumber : ValueObject
{
    public string Value { get; }
    
    /// <summary>
    /// Gets a formatted display version of the phone number for user interfaces.
    /// Shows international format with country code: +1 (555) 123-4567, +44 20 7946 0958, etc.
    /// </summary>
    public string FormattedValue => FormatForDisplay(Value);
    
    private PhoneNumber() 
    {
        Value = string.Empty; // For EF Core
    }
    
    public PhoneNumber(string value)
    {
        ValidateAndThrow(value);
        Value = NormalizePhoneNumber(value);
    }
    
    private static void ValidateAndThrow(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidPhoneNumberException();
            
        var normalizedNumber = NormalizePhoneNumber(value);
        if (!IsValidPhoneNumberFormat(normalizedNumber))
            throw new InvalidPhoneNumberException();
    }
    
    private static bool IsValidPhoneNumberFormat(string normalizedPhoneNumber)
    {
        // Validate the normalized phone number with country code
        // Should be 11-15 digits (country code + national number)
        return normalizedPhoneNumber.Length >= 11 && normalizedPhoneNumber.Length <= 15;
    }
    
    /// <summary>
    /// Normalizes phone number to international format with country code.
    /// Examples: 
    /// - "(555) 123-4567" becomes "15551234567" (US)
    /// - "+1 555 123 4567" becomes "15551234567" (US) 
    /// - "+44 20 7946 0958" becomes "442079460958" (UK)
    /// - "0123456789" becomes "15551234567" (assumes US if no country code)
    /// </summary>
    /// <param name="phoneNumber">The raw phone number string</param>
    /// <returns>Normalized phone number with country code prefix</returns>
    private static string NormalizePhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return string.Empty;
            
        // Extract only digits from the phone number
        var digitsOnly = new string(phoneNumber.Where(char.IsDigit).ToArray());
        
        if (string.IsNullOrEmpty(digitsOnly))
            return string.Empty;
            
        // If already starts with country code (11+ digits), return as-is
        if (digitsOnly.Length >= 11)
        {
            return digitsOnly;
        }
        
        // If 10 digits, assume US number and prepend country code 1
        if (digitsOnly.Length == 10)
        {
            return "1" + digitsOnly;
        }

        // Otherwise treat as invalid (will be rejected by validation)
        return string.Empty;
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
        
    /// <summary>
    /// Formats the normalized phone number for display purposes.
    /// For US numbers (starting with 1): +1 (XXX) XXX-XXXX
    /// For other country codes: +CC XXXX-XXXX or +CC XXXXXXXX
    /// </summary>
    /// <param name="normalizedNumber">The normalized phone number with country code</param>
    /// <returns>Formatted phone number for display</returns>
    private static string FormatForDisplay(string normalizedNumber)
    {
        if (string.IsNullOrEmpty(normalizedNumber))
            return string.Empty;
            
        // US numbers (country code 1): +1 (555) 123-4567
        if (normalizedNumber.Length == 11 && normalizedNumber.StartsWith("1"))
        {
            var areaCode = normalizedNumber.Substring(1, 3);
            var exchange = normalizedNumber.Substring(4, 3);
            var number = normalizedNumber.Substring(7, 4);
            return $"+1 ({areaCode}) {exchange}-{number}";
        }
        
        // UK numbers (country code 44)
        if (normalizedNumber.StartsWith("44"))
        {
            var countryCode = "44";
            var nationalNumber = normalizedNumber.Substring(2);
            if (nationalNumber.Length >= 8)
            {
                return $"+{countryCode} {nationalNumber.Substring(0, 2)} {nationalNumber.Substring(2, 4)} {nationalNumber.Substring(6)}";
            }
            return $"+{countryCode} {nationalNumber}";
        }
        
        // Other country codes: try to extract 1-3 digit country code
        for (int i = 1; i <= 3 && i < normalizedNumber.Length; i++)
        {
            var potentialCountryCode = normalizedNumber.Substring(0, i);
            var nationalNumber = normalizedNumber.Substring(i);
            
            // For most countries, format as +CC XXXX-XXXX or similar
            if (nationalNumber.Length >= 8)
            {
                var firstPart = nationalNumber.Substring(0, 4);
                var secondPart = nationalNumber.Substring(4);
                return $"+{potentialCountryCode} {firstPart}-{secondPart}";
            }
            else if (nationalNumber.Length >= 4)
            {
                return $"+{potentialCountryCode} {nationalNumber}";
            }
        }
        
        // Fallback: just add + prefix
        return $"+{normalizedNumber}";
    }
}
