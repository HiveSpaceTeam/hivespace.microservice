using System.Collections.Generic;
using System.Text.RegularExpressions;
using HiveSpace.Domain.Shared.Entities;
using HiveSpace.UserService.Domain.Exceptions;

namespace HiveSpace.UserService.Domain.Aggregates.User;

public class Email : ValueObject
{
    public string Value { get; }

    private Email() 
    {
        Value = string.Empty; // For EF Core
    }

    private Email(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidEmailException("Email address cannot be empty.");
        if (!IsValidEmail(value))
            throw new InvalidEmailException("Invalid email address format.");
        
        Value = value.ToLowerInvariant(); // Store in consistent format
    }

    private static bool IsValidEmail(string email)
    {
        // Basic regex for email validation. Consider more robust solutions for production.
        return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public static implicit operator string(Email email) => email.Value;
    public static explicit operator Email(string value) => new(value);
    
    // Factory methods for better creation control
    public static Email Create(string value) => new Email(value);
    public static Email? CreateOrDefault(string? value) => 
        string.IsNullOrWhiteSpace(value) ? null : new Email(value);
}