using FluentAssertions;
using HiveSpace.UserService.Domain.Aggregates.User;
using HiveSpace.UserService.Domain.Enums;
using HiveSpace.UserService.Domain.Exceptions;
using HiveSpace.UserService.Tests.Fixtures;
using Xunit;

namespace HiveSpace.UserService.Tests.Application.Addresses;

public class AddAddressCommandHandlerTests : IClassFixture<UserServiceFixture>
{
    private readonly UserServiceFixture _fixture;

    public AddAddressCommandHandlerTests(UserServiceFixture fixture) => _fixture = fixture;

    [Fact]
    public void Handle_WithValidAddress_AddsToUserAddressCollection()
    {
        var user = NewUser("add-address@hivespace.local");

        user.AddAddress("Name", "0901234567", "123 Main St", "Ward 1", "Hanoi", "VN", null, AddressType.Home, true);

        user.Addresses.Should().ContainSingle(a => a.Street == "123 Main St");
    }

    [Fact]
    public void Handle_WithPhoneNumberExceedingMaxLength_ThrowsDomainException()
    {
        var user = NewUser("add-address-invalid@hivespace.local");
        var longPhone = new string('0', 21);

        var act = () => user.AddAddress("Name", longPhone, "Street", "Ward", "Hanoi", "VN", null, AddressType.Home);

        act.Should().Throw<InvalidAddressException>();
    }

    private static User NewUser(string email) =>
        User.CreateProfile(Guid.NewGuid(), Email.Create(email), email, "Address User");
}
