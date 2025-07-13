using HiveSpace.Domain.Shared;
using HiveSpace.IdentityService.Domain.Aggregates.Enums;
using HiveSpace.IdentityService.Domain.Exceptions;
using Microsoft.AspNetCore.Identity;

namespace HiveSpace.IdentityService.Domain.Aggregates;

// Add profile data for application users by adding properties to the ApplicationUser class
public class ApplicationUser : IdentityUser, IAggregateRoot, IAuditable, ISoftDeletable
{
    public string FullName { get; private set; }
    public Gender? Gender { get; private set; }
    public DateTime? DateOfBirth { get; private set; }

    private readonly List<Address> _addresses = [];
    public IReadOnlyCollection<Address> Addresses => _addresses;

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? UpdatedAt { get; private set; }

    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }

    private List<IDomainEvent> _domainEvents = [];
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    private ApplicationUser() : base()
    {
    }

    public ApplicationUser(
        string email,
        string userName,
        string fullName,
        string? phoneNumber,
        Gender? gender = null,
        DateTime? dob = null)
    {
        PhoneNumber = phoneNumber;
        UserName = userName;
        FullName = fullName;
        Email = email;
        Gender = gender;
        DateOfBirth = dob;
    }

    public Address AddAddress(AddressProps props)
    {
        var address = new Address(props);
        _addresses.Add(address);
        return address;
    }

    public void RemoveAddress(Guid addressId)
    {
        for (int i = 0; i < _addresses.Count; i++)
        {
            if (_addresses[i].Id == addressId)
            {
                _addresses.RemoveAt(i);
                return;
            }
        }
        throw new AddressNotFoundException();
    }

    public Address UpdateAddress(Guid addressId, AddressProps props)
    {
        var address = _addresses.Find(x => x.Id == addressId) ?? throw new AddressNotFoundException();
        address.UpdateAddress(props);
        return address;
    }

    public void UpdateUserInfo(string? userName, string? fullName, string? email, string? phoneNumber, Gender? gender, DateTime? dob)
    {
        if (!string.IsNullOrWhiteSpace(userName)) UserName = userName;
        if (!string.IsNullOrWhiteSpace(fullName)) FullName = fullName;
        if (!string.IsNullOrWhiteSpace(email)) Email = email;
        if (!string.IsNullOrWhiteSpace(phoneNumber) && PhoneNumber != phoneNumber) PhoneNumber = phoneNumber;
        if (gender is not null) Gender = gender;
        if (dob is not null) DateOfBirth = dob;
    }

    public void SetDefaultAddress(Guid addressId)
    {
        Address? address = null;
        foreach (var addr in _addresses)
        {
            if (addr.Id == addressId)
                address = addr;
            addr.SetDefault(false);
        }
        if (address is null)
            throw new AddressNotFoundException();
        address.SetDefault(true);
    }

    public void AddDomainEvent(IDomainEvent eventItem)
    {
        _domainEvents = _domainEvents ?? [];
        _domainEvents.Add(eventItem);
    }

    public void RemoveDomainEvent(IDomainEvent eventItem)
    {
        _domainEvents?.Remove(eventItem);
    }

    public void ClearDomainEvents()
    {
        _domainEvents?.Clear();
    }
}