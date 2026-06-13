using FluentAssertions;
using HiveSpace.UserService.Domain.Aggregates.User;
using HiveSpace.UserService.Domain.Enums;
using HiveSpace.UserService.Domain.Exceptions;
using Xunit;

namespace HiveSpace.UserService.Tests.Domain;

public class UserTests
{
    private static Email ValidEmail(string addr = "test@example.com") => Email.Create(addr);

    [Fact]
    public void Create_WithUsernameTooShort_ThrowsInvalidUserInformationException()
    {
        var act = () => User.CreateProfile(Guid.NewGuid(), ValidEmail(), "ab", "Valid Name");
        act.Should().Throw<InvalidUserInformationException>();
    }

    [Fact]
    public void Create_WithUsernameTooLong_ThrowsInvalidUserInformationException()
    {
        var longName = new string('a', 51);
        var act = () => User.CreateProfile(Guid.NewGuid(), ValidEmail(), longName, "Valid Name");
        act.Should().Throw<InvalidUserInformationException>();
    }

    [Fact]
    public void Create_WithUsernameContainingInvalidCharacters_ThrowsInvalidUserInformationException()
    {
        var act = () => User.CreateProfile(Guid.NewGuid(), ValidEmail(), "user#invalid!", "Valid Name");
        act.Should().Throw<InvalidUserInformationException>();
    }

    [Fact]
    public void UpdateProfile_WithValidFields_ChangesStoredValues()
    {
        var user = User.CreateProfile(Guid.NewGuid(), ValidEmail(), "validuser", "Old Name");
        user.UpdateProfile("New Full Name", null, null, null, "newusername");
        user.FullName.Should().Be("New Full Name");
        user.UserName.Should().Be("newusername");
    }

    [Fact]
    public void MarkAddressAsDefault_ClearsPreviousDefaultFlag()
    {
        var user = User.CreateProfile(Guid.NewGuid(), ValidEmail(), "validuser2", "Valid User");
        var first = user.AddAddress("Name", "0900000000", "Street 1", "Ward", "Hanoi", "VN", null, AddressType.Home, true);
        var second = user.AddAddress("Name", "0900000000", "Street 2", "Ward", "Hanoi", "VN", null, AddressType.Work);

        user.MarkAddressAsDefault(second.Id);

        first.IsDefault.Should().BeFalse();
        second.IsDefault.Should().BeTrue();
    }

    [Fact]
    public void RemoveAddress_WhenOnlyAddress_ThrowsCannotRemoveOnlyAddressException()
    {
        var user = User.CreateProfile(Guid.NewGuid(), ValidEmail("only@example.com"), "onlyuser", "Only User");
        var address = user.AddAddress("Name", "0900000000", "Street 1", "Ward", "Hanoi", "VN", null, AddressType.Home);

        var act = () => user.RemoveAddress(address.Id);
        act.Should().Throw<CannotRemoveOnlyAddressException>();
    }
}
