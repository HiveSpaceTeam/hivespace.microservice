using FluentAssertions;
using HiveSpace.UserService.Domain.Aggregates.User;
using HiveSpace.UserService.Domain.Enums;
using HiveSpace.UserService.Domain.Exceptions;
using HiveSpace.UserService.Tests.Fixtures;
using Xunit;

namespace HiveSpace.UserService.Tests.Application.Addresses;

public class RemoveAddressCommandHandlerTests : IClassFixture<UserServiceFixture>
{
    private readonly UserServiceFixture _fixture;

    public RemoveAddressCommandHandlerTests(UserServiceFixture fixture) => _fixture = fixture;

    [Fact]
    public void Handle_WithNonDefaultAddress_RemovesFromCollection()
    {
        var user = NewUser("remove-address@hivespace.local");
        user.AddAddress("Name", "0900000000", "Default St", "Ward", "City", "VN", null, AddressType.Home, true);
        var nonDefault = user.AddAddress("Name", "0900000000", "Other St", "Ward", "City", "VN", null, AddressType.Work);

        user.RemoveAddress(nonDefault.Id);

        user.Addresses.Should().NotContain(a => a.Id == nonDefault.Id);
    }

    [Fact]
    public void Handle_WithDefaultAddress_ThrowsDomainException()
    {
        var user = NewUser("remove-default@hivespace.local");
        var defaultAddr = user.AddAddress("Name", "0900000000", "Main St", "Ward", "City", "VN", null, AddressType.Home, true);
        user.AddAddress("Name", "0900000000", "Other St", "Ward", "City", "VN", null, AddressType.Work);

        var act = () => user.RemoveAddress(defaultAddr.Id);

        act.Should().Throw<CannotRemoveDefaultAddressException>();
    }

    private static User NewUser(string email) =>
        User.CreateProfile(Guid.NewGuid(), Email.Create(email), email, "Address User");
}
