using HiveSpace.Domain.Shared.Entities;
using HiveSpace.Domain.Shared.Interfaces;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.UserService.Domain.Enums;
using HiveSpace.UserService.Domain.Exceptions;

namespace HiveSpace.UserService.Domain.Aggregates.User;

public class Address : Entity<Guid>, IAuditable
{
    public string FullName { get; private set; }  // Primitive value
    public string PhoneNumber { get; private set; }
    public string Street { get; private set; }
    public string District { get; private set; }
    public string Province { get; private set; }
    public string Country { get; private set; }
    public string? ZipCode { get; private set; }
    public AddressType AddressType { get; private set; }
    public bool IsDefault { get; private set; }
    
    // Audit
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    
    public Address(string fullName, string phoneNumber, string street, string district, 
        string province, string country, string? zipCode, AddressType addressType)
    {
        ValidateAndThrow(fullName, phoneNumber, street, district, province, country);
        
        FullName = fullName;
        PhoneNumber = phoneNumber;
        Street = street;
        District = district;
        Province = province;
        Country = country;
        ZipCode = zipCode;
        AddressType = addressType;
    }
    
    private static void ValidateAndThrow(string? fullName, string? phoneNumber, string? street, 
        string? district, string? province, string? country)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new InvalidAddressException(nameof(FullName));
        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new InvalidAddressException(nameof(PhoneNumber));
        if (string.IsNullOrWhiteSpace(street))
            throw new InvalidAddressException(nameof(Street));
        if (string.IsNullOrWhiteSpace(district))
            throw new InvalidAddressException(nameof(District));
        if (string.IsNullOrWhiteSpace(province))
            throw new InvalidAddressException(nameof(Province));
        if (string.IsNullOrWhiteSpace(country))
            throw new InvalidAddressException(nameof(Country));
        
        // Length validations
        if (fullName.Length > 100)
            throw new InvalidAddressException(nameof(FullName));
        if (phoneNumber.Length > 20)
            throw new InvalidAddressException(nameof(PhoneNumber));
        if (street.Length > 200)
            throw new InvalidAddressException(nameof(Street));
        if (district.Length > 100)
            throw new InvalidAddressException(nameof(District));
        if (province.Length > 100)
            throw new InvalidAddressException(nameof(Province));
        if (country.Length > 100)
            throw new InvalidAddressException(nameof(Country));
    }
    
    public void UpdateDetails(string? fullName, string? phoneNumber, string? street, 
        string? district, string? province, string? country, string? zipCode, AddressType? addressType)
    {
        if (!string.IsNullOrWhiteSpace(fullName)) FullName = fullName;
        if (!string.IsNullOrWhiteSpace(phoneNumber)) PhoneNumber = phoneNumber;
        if (!string.IsNullOrWhiteSpace(street)) Street = street;
        if (!string.IsNullOrWhiteSpace(district)) District = district;
        if (!string.IsNullOrWhiteSpace(province)) Province = province;
        if (!string.IsNullOrWhiteSpace(country)) Country = country;
        if (zipCode != null) ZipCode = zipCode;
        if (addressType != null) AddressType = addressType.Value;
    }
    
    internal void SetAsDefault()
    {
        IsDefault = true;
    }
    
    internal void RemoveDefaultStatus()
    {
        IsDefault = false;
    }
    
    public bool CanBeRemoved()
    {
        return !IsDefault;
    }
}
