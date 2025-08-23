using HiveSpace.Domain.Shared.Entities;
using HiveSpace.UserService.Domain.Exceptions;

namespace HiveSpace.UserService.Domain.Aggregates.User;

public class DateOfBirth : ValueObject
{
    public DateTimeOffset Value { get; }
    
    private DateOfBirth() { } // For EF Core
    
    public DateOfBirth(DateTimeOffset value)
    {
        ValidateAndThrow(value);
        Value = value;
    }
    
    private static void ValidateAndThrow(DateTimeOffset value)
    {
        if (value > DateTimeOffset.UtcNow)
            throw new InvalidDateOfBirthException();
            
        if (value < DateTimeOffset.UtcNow.AddYears(-120))
            throw new InvalidDateOfBirthException();
    }
    
    private static bool IsValidDateOfBirth(DateTimeOffset value)
    {
        return value <= DateTimeOffset.UtcNow && value >= DateTimeOffset.UtcNow.AddYears(-120);
    }
    
    public int Age
    {
        get
        {
            var today = DateTime.UtcNow.Date;
            var dobDate = Value.UtcDateTime.Date;
            var age = today.Year - dobDate.Year;
            if (dobDate > today.AddYears(-age)) age--;
            return age;
        }
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
    
    public static implicit operator DateTimeOffset(DateOfBirth dateOfBirth) => dateOfBirth.Value;
    public static explicit operator DateOfBirth(DateTimeOffset value) => new(value);
    
    // Factory methods for better creation control
    public static DateOfBirth Create(DateTimeOffset value) => new(value);
    public static DateOfBirth? CreateOrDefault(DateTimeOffset? value) => 
        value == null ? null : new DateOfBirth(value.Value);
}
