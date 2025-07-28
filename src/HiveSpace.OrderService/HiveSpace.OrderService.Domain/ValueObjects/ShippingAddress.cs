using HiveSpace.Domain.Shared.Entities;

namespace HiveSpace.OrderService.Domain.ValueObjects;

public class ShippingAddress : ValueObject
{
    public string FullName { get; private set; }
    public PhoneNumber PhoneNumber { get; private set; }
    public string OtherDetails { get; private set; }
    public Address Address { get; private set; }

    public ShippingAddress(string fullName, string phoneNumber, string otherDetails, string street, string ward, string district, string province, string country)
    {
        FullName = fullName;
        PhoneNumber = new PhoneNumber(phoneNumber);
        OtherDetails = otherDetails;
        Address = new Address(street, ward, district, province, country);
    }

    private ShippingAddress()
    {
        FullName = string.Empty;
        PhoneNumber = null!;
        OtherDetails = string.Empty;
        Address = null!;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return FullName;
        yield return PhoneNumber;
        yield return OtherDetails;
        yield return Address;
    }
}