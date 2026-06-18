using FluentAssertions;
using HiveSpace.UserService.Domain.Aggregates.User;
using HiveSpace.UserService.Domain.Enums;
using HiveSpace.UserService.Domain.Exceptions;
using Xunit;

namespace HiveSpace.UserService.Tests.Domain;

public class AddressTests
{
    private static User NewUser(string email = "addr@example.com") =>
        User.CreateProfile(Guid.NewGuid(), Email.Create(email), "addruser", "Addr User");

    [Fact]
    public void SetAsDefault_ClearsPreviousDefaultFlag()
    {
        var user = NewUser();
        var first = user.AddAddress("Name", "0900000000", "St 1", "Ward", "City", "VN", null, AddressType.Home, true);
        var second = user.AddAddress("Name", "0900000000", "St 2", "Ward", "City", "VN", null, AddressType.Work);

        user.MarkAddressAsDefault(second.Id);

        first.IsDefault.Should().BeFalse();
        second.IsDefault.Should().BeTrue();
    }

    [Fact]
    public void Create_WithPhoneNumberExceedingMaxLength_ThrowsDomainException()
    {
        var longPhone = new string('0', 21);
        var act = () => new Address("Name", longPhone, "Street", "Commune", "Province", "VN", null, AddressType.Home);
        act.Should().Throw<InvalidAddressException>();
    }

    [Fact]
    public void Create_WithStreetExceedingMaxLength_ThrowsDomainException()
    {
        var longStreet = new string('a', 201);
        var act = () => new Address("Name", "0900000000", longStreet, "Commune", "Province", "VN", null, AddressType.Home);
        act.Should().Throw<InvalidAddressException>();
    }

    [Fact]
    public void Create_WithNullFullName_ThrowsDomainException()
    {
        var act = () => new Address(null!, "0900000000", "Street", "Commune", "Province", "VN", null, AddressType.Home);
        act.Should().Throw<InvalidAddressException>();
    }

    [Fact]
    public void Create_WithFullNameExceedingMaxLength_ThrowsDomainException()
    {
        var longName = new string('a', 101);
        var act = () => new Address(longName, "0900000000", "Street", "Commune", "Province", "VN", null, AddressType.Home);
        act.Should().Throw<InvalidAddressException>();
    }

    [Fact]
    public void Create_WithNullPhoneNumber_ThrowsDomainException()
    {
        var act = () => new Address("Name", null!, "Street", "Commune", "Province", "VN", null, AddressType.Home);
        act.Should().Throw<InvalidAddressException>();
    }

    [Fact]
    public void Create_WithNullStreet_ThrowsDomainException()
    {
        var act = () => new Address("Name", "0900000000", null!, "Commune", "Province", "VN", null, AddressType.Home);
        act.Should().Throw<InvalidAddressException>();
    }

    [Fact]
    public void Create_WithProvinceExceedingMaxLength_ThrowsDomainException()
    {
        var longProvince = new string('a', 101);
        var act = () => new Address("Name", "0900000000", "Street", "Commune", longProvince, "VN", null, AddressType.Home);
        act.Should().Throw<InvalidAddressException>();
    }

    [Fact]
    public void UpdateDetails_WithAllNullArguments_ChangesNothing()
    {
        var addr = new Address("Original Name", "0900000000", "Original St", "Ward", "City", "VN", null, AddressType.Home);
        addr.UpdateDetails(null, null, null, null, null, null, null, null);
        addr.FullName.Should().Be("Original Name");
        addr.Street.Should().Be("Original St");
    }

    [Fact]
    public void UpdateDetails_WithNewValues_ChangesProperties()
    {
        var addr = new Address("Old Name", "0900000000", "Old St", "Ward", "City", "VN", null, AddressType.Home);
        addr.UpdateDetails("New Name", null, "New St", null, null, null, null, AddressType.Work);
        addr.FullName.Should().Be("New Name");
        addr.Street.Should().Be("New St");
        addr.AddressType.Should().Be(AddressType.Work);
    }

    [Fact]
    public void CanBeRemoved_WhenNotDefault_ReturnsTrue()
    {
        var addr = new Address("Name", "0900000000", "Street", "Commune", "Province", "VN", null, AddressType.Home);
        addr.CanBeRemoved().Should().BeTrue();
    }

    [Fact]
    public void CanBeRemoved_AfterSetAsDefault_ReturnsFalse()
    {
        var addr = new Address("Name", "0900000000", "Street", "Commune", "Province", "VN", null, AddressType.Home);
        addr.SetAsDefault();
        addr.CanBeRemoved().Should().BeFalse();
    }

    [Fact]
    public void Create_WithNullCommune_ThrowsInvalidAddressException()
    {
        var act = () => new Address("Name", "0900000000", "Street", null!, "Province", "VN", null, AddressType.Home);
        act.Should().Throw<InvalidAddressException>();
    }

    [Fact]
    public void Create_WithNullProvince_ThrowsInvalidAddressException()
    {
        var act = () => new Address("Name", "0900000000", "Street", "Commune", null!, "VN", null, AddressType.Home);
        act.Should().Throw<InvalidAddressException>();
    }

    [Fact]
    public void Create_WithNullCountry_ThrowsInvalidAddressException()
    {
        var act = () => new Address("Name", "0900000000", "Street", "Commune", "Province", null!, null, AddressType.Home);
        act.Should().Throw<InvalidAddressException>();
    }

    [Fact]
    public void Create_WithCommuneExceedingMaxLength_ThrowsInvalidAddressException()
    {
        var longCommune = new string('a', 101);
        var act = () => new Address("Name", "0900000000", "Street", longCommune, "Province", "VN", null, AddressType.Home);
        act.Should().Throw<InvalidAddressException>();
    }

    [Fact]
    public void Create_WithCountryExceedingMaxLength_ThrowsInvalidAddressException()
    {
        var longCountry = new string('a', 101);
        var act = () => new Address("Name", "0900000000", "Street", "Commune", "Province", longCountry, null, AddressType.Home);
        act.Should().Throw<InvalidAddressException>();
    }

    [Fact]
    public void UpdateDetails_WithPhoneNumberAndCommune_UpdatesThoseFields()
    {
        var addr = new Address("Name", "0900000000", "Street", "Ward", "City", "VN", null, AddressType.Home);

        addr.UpdateDetails(null, "0911111111", null, "New Ward", null, null, null, null);

        addr.PhoneNumber.Should().Be("0911111111");
        addr.Commune.Should().Be("New Ward");
    }

    [Fact]
    public void UpdateDetails_WithProvinceCountryAndZipCode_UpdatesThoseFields()
    {
        var addr = new Address("Name", "0900000000", "Street", "Ward", "City", "VN", null, AddressType.Home);

        addr.UpdateDetails(null, null, null, null, "New City", "US", "90210", null);

        addr.Province.Should().Be("New City");
        addr.Country.Should().Be("US");
        addr.ZipCode.Should().Be("90210");
    }
}
