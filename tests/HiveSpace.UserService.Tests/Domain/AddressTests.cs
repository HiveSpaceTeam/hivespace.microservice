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
}
