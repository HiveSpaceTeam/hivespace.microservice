using FluentAssertions;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.Testing.Shared.Doubles;
using HiveSpace.UserService.Application.UserAddresses.Commands.SetDefaultUserAddress;
using HiveSpace.UserService.Application.UserAddresses.Queries.GetUserAddresses;
using HiveSpace.UserService.Domain.Aggregates.User;
using HiveSpace.UserService.Domain.Enums;
using HiveSpace.UserService.Infrastructure.Repositories;
using HiveSpace.UserService.Tests.Fixtures;
using Xunit;

namespace HiveSpace.UserService.Tests.Application.Addresses;

public class SetDefaultUserAddressCommandHandlerTests : IClassFixture<UserServiceFixture>
{
    private readonly UserServiceFixture _fixture;

    public SetDefaultUserAddressCommandHandlerTests(UserServiceFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Handle_SetNewDefault_ClearsPreviousDefaultFlag()
    {
        var user = NewUser("set-default@hivespace.local");
        var first = user.AddAddress("Name", "0900000000", "St 1", "Ward", "City", "VN", null, AddressType.Home, true);
        var second = user.AddAddress("Name", "0900000000", "St 2", "Ward", "City", "VN", null, AddressType.Work);
        _fixture.DbContext.Users.Add(user);
        await _fixture.DbContext.SaveChangesAsync();

        var repo = new SqlUserRepository(_fixture.DbContext);
        var handler = new SetDefaultUserAddressCommandHandler(new FakeUserContext { UserId = user.Id }, repo);
        await handler.Handle(new SetDefaultUserAddressCommand(second.Id), CancellationToken.None);

        var getHandler = new GetUserAddressesQueryHandler(new FakeUserContext { UserId = user.Id }, repo);
        var addresses = await getHandler.Handle(new GetUserAddressesQuery(), CancellationToken.None);

        addresses.Single(a => a.Id == first.Id).IsDefault.Should().BeFalse();
        addresses.Single(a => a.Id == second.Id).IsDefault.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_OnlyOneAddressCanBeDefault()
    {
        var user = NewUser("single-default@hivespace.local");
        user.AddAddress("Name", "0900000000", "St 1", "Ward", "City", "VN", null, AddressType.Home, true);
        user.AddAddress("Name", "0900000000", "St 2", "Ward", "City", "VN", null, AddressType.Work);
        user.AddAddress("Name", "0900000000", "St 3", "Ward", "City", "VN", null, AddressType.Work);
        _fixture.DbContext.Users.Add(user);
        await _fixture.DbContext.SaveChangesAsync();

        var third = user.Addresses.Last();
        var repo = new SqlUserRepository(_fixture.DbContext);
        var handler = new SetDefaultUserAddressCommandHandler(new FakeUserContext { UserId = user.Id }, repo);
        await handler.Handle(new SetDefaultUserAddressCommand(third.Id), CancellationToken.None);

        var getHandler = new GetUserAddressesQueryHandler(new FakeUserContext { UserId = user.Id }, repo);
        var addresses = await getHandler.Handle(new GetUserAddressesQuery(), CancellationToken.None);

        addresses.Count(a => a.IsDefault).Should().Be(1);
    }

    [Fact]
    public async Task Handle_WithNonExistentUser_ThrowsNotFoundException()
    {
        var handler = new SetDefaultUserAddressCommandHandler(
            new FakeUserContext { UserId = Guid.NewGuid() },
            new SqlUserRepository(_fixture.DbContext));

        var act = () => handler.Handle(new SetDefaultUserAddressCommand(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    private static User NewUser(string email) =>
        User.CreateProfile(Guid.NewGuid(), Email.Create(email), email, "Address User");
}
