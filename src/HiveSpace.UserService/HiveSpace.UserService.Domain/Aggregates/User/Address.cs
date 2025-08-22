using HiveSpace.Domain.Shared.Entities;
using HiveSpace.UserService.Domain.Enums;

namespace HiveSpace.UserService.Domain.Aggregates.User;

public class Address : Entity<Guid>
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
    public DateTimeOffset UpdatedAt { get; private set; }
    
    public Address(string fullName, string phoneNumber, string street, string district, 
        string province, string country, string? zipCode, AddressType addressType)
    {
        FullName = fullName;
        PhoneNumber = phoneNumber;
        Street = street;
        District = district;
        Province = province;
        Country = country;
        ZipCode = zipCode;
        AddressType = addressType;
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
