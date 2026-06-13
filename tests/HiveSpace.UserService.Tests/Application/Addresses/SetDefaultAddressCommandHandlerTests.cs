using FluentAssertions;
using HiveSpace.UserService.Domain.Aggregates.User;
using HiveSpace.UserService.Domain.Enums;
using HiveSpace.UserService.Tests.Fixtures;
using Xunit;

namespace HiveSpace.UserService.Tests.Application.Addresses;

public class SetDefaultAddressCommandHandlerTests : IClassFixture<UserServiceFixture>
{
    private readonly UserServiceFixture _fixture;

    public SetDefaultAddressCommandHandlerTests(UserServiceFixture fixture) => _fixture = fixture;

    [Fact]
    public void Handle_SetNewDefault_ClearsPreviousDefaultFlag()
    {
        var user = NewUser("set-default@hivespace.local");
        var first = user.AddAddress("Name", "0900000000", "St 1", "Ward", "City", "VN", null, AddressType.Home, true);
        var second = user.AddAddress("Name", "0900000000", "St 2", "Ward", "City", "VN", null, AddressType.Work);

        user.MarkAddressAsDefault(second.Id);

        first.IsDefault.Should().BeFalse();
        second.IsDefault.Should().BeTrue();
    }

    [Fact]
    public void Handle_OnlyOneAddressCanBeDefault()
    {
        var user = NewUser("single-default@hivespace.local");
        user.AddAddress("Name", "0900000000", "St 1", "Ward", "City", "VN", null, AddressType.Home, true);
        user.AddAddress("Name", "0900000000", "St 2", "Ward", "City", "VN", null, AddressType.Work);
        user.AddAddress("Name", "0900000000", "St 3", "Ward", "City", "VN", null, AddressType.Work);

        var third = user.Addresses.Last();
        user.MarkAddressAsDefault(third.Id);

        user.Addresses.Count(a => a.IsDefault).Should().Be(1);
    }

    private static User NewUser(string email) =>
        User.CreateProfile(Guid.NewGuid(), Email.Create(email), email, "Address User");
}
