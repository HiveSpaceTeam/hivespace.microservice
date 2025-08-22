using HiveSpace.Domain.Shared.Entities;
using HiveSpace.UserService.Domain.Exceptions;

namespace HiveSpace.UserService.Domain.Aggregates.User;

public class DateOfBirth : ValueObject
{
    public DateTimeOffset Value { get; }
    
    private DateOfBirth() { } // For EF Core
    
    public DateOfBirth(DateTimeOffset value)
    {
        if (value > DateTimeOffset.UtcNow)
            throw new InvalidDateOfBirthException();
            
        if (value < DateTimeOffset.UtcNow.AddYears(-120))
            throw new InvalidDateOfBirthException();
            
        Value = value;
    }
    
    public int Age => DateTimeOffset.UtcNow.Year - Value.Year;
    
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
