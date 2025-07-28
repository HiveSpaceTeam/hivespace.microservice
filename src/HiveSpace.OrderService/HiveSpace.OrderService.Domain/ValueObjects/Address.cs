using HiveSpace.Domain.Shared.Entities;

namespace HiveSpace.OrderService.Domain.ValueObjects;

public class Address : ValueObject
{
    public string Street { get; private set; }
    public string Ward { get; private set; }
    public string District { get; private set; }
    public string Province { get; private set; }
    public string Country { get; private set; }

    public Address(string street, string ward, string district, string province, string country)
    {
        Street = street;
        Ward = ward;
        District = district;
        Province = province;
        Country = country;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Street;
        yield return Ward;
        yield return District;
        yield return Province;
        yield return Country;
    }
}