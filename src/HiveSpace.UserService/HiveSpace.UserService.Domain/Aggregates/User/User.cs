using HiveSpace.Domain.Shared.Entities;
using HiveSpace.Domain.Shared.Interfaces;
using HiveSpace.UserService.Domain.Enums;
using HiveSpace.UserService.Domain.Exceptions;

namespace HiveSpace.UserService.Domain.Aggregates.User;

public class User : AggregateRoot<Guid>, IAuditable
{
    // Identity
    public Email Email { get; private set; }
    public string UserName { get; private set; }
    public string PasswordHash { get; private set; }
    public UserStatus Status { get; private set; }
    
    // Store relationship
    public Guid? StoreId { get; private set; }
    
    // Profile
    public string FullName { get; private set; }  // Primitive value
    public PhoneNumber? PhoneNumber { get; private set; }
    public DateOfBirth? DateOfBirth { get; private set; }
    public Gender? Gender { get; private set; }
    
    // Relationships
    private readonly List<Address> _addresses;
    public IReadOnlyCollection<Address> Addresses => _addresses.AsReadOnly();
    
    // Audit
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    public DateTimeOffset? LastLoginAt { get; private set; }
    
    private User() 
    {
        _addresses = new List<Address>();
        Email = null!;
        UserName = string.Empty;
        PasswordHash = null!;
        FullName = string.Empty;
    }

    // Domain factory method - Internal to force creation through UserManager
    internal static User Create(Email email, string userName, string passwordHash, string fullName)
    {
        var user = new User
        {
            Email = email,
            UserName = userName,
            PasswordHash = passwordHash,
            FullName = fullName,
            Status = UserStatus.Active,
            StoreId = null
        };
        

        return user;
    }
    
    public void UpdateProfile(string? fullName, PhoneNumber? phoneNumber, DateOfBirth? dateOfBirth, Gender? gender)
    {
        if (!string.IsNullOrWhiteSpace(fullName)) FullName = fullName;
        if (phoneNumber != null) PhoneNumber = phoneNumber;
        if (dateOfBirth != null) DateOfBirth = dateOfBirth;
        if (gender != null) Gender = gender;
        
        UpdatedAt = DateTimeOffset.UtcNow;
    }
    
    public void AssignStore(Guid storeId)
    {
        StoreId = storeId;
        UpdatedAt = DateTimeOffset.UtcNow;

    }
    
    public void RemoveStore()
    {
        StoreId = null;
        UpdatedAt = DateTimeOffset.UtcNow;

    }
    
    public void AddAddress(string fullName, string phoneNumber, string street, string district, 
        string province, string country, string? zipCode, AddressType addressType, bool setAsDefault = false)
    {
        var address = new Address(fullName, phoneNumber, street, district, province, country, zipCode, addressType);
        
        if (setAsDefault)
        {
            foreach (var existingAddress in _addresses)
            {
                existingAddress.RemoveDefaultStatus();
            }
            address.SetAsDefault();
        }
        
        _addresses.Add(address);
        UpdatedAt = DateTimeOffset.UtcNow;

    }
    
    public void UpdateAddress(Guid addressId, string? fullName, string? phoneNumber, string? street, 
        string? district, string? province, string? country, string? zipCode, AddressType? addressType)
    {
        var address = _addresses.FirstOrDefault(a => a.Id == addressId) ?? throw new AddressNotFoundException();
        address.UpdateDetails(fullName, phoneNumber, street, district, province, country, zipCode, addressType);
        UpdatedAt = DateTimeOffset.UtcNow;
    }
    
    public void RemoveAddress(Guid addressId)
    {
        var address = _addresses.FirstOrDefault(a => a.Id == addressId) ?? throw new AddressNotFoundException();
            
        if (address.IsDefault)
            throw new CannotRemoveDefaultAddressException();
            
        if (_addresses.Count == 1)
            throw new CannotRemoveOnlyAddressException();
            
        _addresses.Remove(address);
        UpdatedAt = DateTimeOffset.UtcNow;
    }
    
    public void MarkAddressAsDefault(Guid addressId)
    {
        var targetAddress = _addresses.FirstOrDefault(a => a.Id == addressId) ?? throw new AddressNotFoundException();
        
        foreach (var address in _addresses)
        {
            address.RemoveDefaultStatus();
        }
        
        targetAddress.SetAsDefault();
        UpdatedAt = DateTimeOffset.UtcNow;
    }
    
    public void Activate()
    {
        Status = UserStatus.Active;
    }
    
    public void Deactivate()
    {
        Status = UserStatus.Inactive;
    }
    
    public void UpdateLastLogin()
    {
        LastLoginAt = DateTimeOffset.UtcNow;
    }
    
    public bool IsCustomer => StoreId == null;
    public bool IsSeller => StoreId != null;
}