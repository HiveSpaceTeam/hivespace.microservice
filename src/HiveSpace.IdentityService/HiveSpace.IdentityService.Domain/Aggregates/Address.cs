using HiveSpace.Domain.Shared;

namespace HiveSpace.IdentityService.Domain.Aggregates;

public sealed class Address : Entity<Guid>, IAuditable
{
    public string FullName { get; private set; }
    public string Street { get; private set; }
    public string Ward { get; private set; }
    public string District { get; private set; }
    public string Province { get; private set; }
    public string Country { get; private set; }
    public string? ZipCode { get; private set; }
    public string? PhoneNumber { get; private set; }
    public bool IsDefault { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }

    private Address() { }

    public Address(AddressProps props)
    {
        UpdateAddress(props);
    }

    internal void SetDefault(bool isDefault) => IsDefault = isDefault;

    public void UpdateAddress(AddressProps props)
    {
        FullName = props.FullName;
        Street = props.Street;
        Ward = props.Ward;
        District = props.District;
        Province = props.Province;
        Country = props.Country;
        ZipCode = props.ZipCode;
        PhoneNumber = props.PhoneNumber;
    }
}

public class AddressProps
{
    public string FullName { get; set; } = default!;
    public string Street { get; set; } = default!;
    public string Ward { get; set; } = default!;
    public string District { get; set; } = default!;
    public string Province { get; set; } = default!;
    public string Country { get; set; } = default!;
    public string? ZipCode { get; set; } = default!;
    public string PhoneNumber { get; set; } = default!;
} 