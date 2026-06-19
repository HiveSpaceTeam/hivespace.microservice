using FluentAssertions;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.Testing.Shared.Doubles;
using HiveSpace.UserService.Application.UserAddresses.Commands.DeleteUserAddress;
using HiveSpace.UserService.Application.UserAddresses.Queries.GetUserAddresses;
using HiveSpace.UserService.Domain.Aggregates.User;
using HiveSpace.UserService.Domain.Enums;
using HiveSpace.UserService.Domain.Exceptions;
using HiveSpace.UserService.Infrastructure.Repositories;
using HiveSpace.UserService.Tests.Fixtures;
using Xunit;

namespace HiveSpace.UserService.Tests.Application.Addresses;

public class DeleteUserAddressCommandHandlerTests : IClassFixture<UserServiceFixture>
{
    private readonly UserServiceFixture _fixture;

    public DeleteUserAddressCommandHandlerTests(UserServiceFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Handle_WithNonDefaultAddress_RemovesFromCollection()
    {
        var user = NewUser("remove-address@hivespace.local");
        user.AddAddress("Name", "0900000000", "Default St", "Ward", "City", "VN", null, AddressType.Home, true);
        var nonDefault = user.AddAddress("Name", "0900000000", "Other St", "Ward", "City", "VN", null, AddressType.Work);
        _fixture.DbContext.Users.Add(user);
        await _fixture.DbContext.SaveChangesAsync();

        var repo = new SqlUserRepository(_fixture.DbContext);
        var deleteHandler = new DeleteUserAddressCommandHandler(new FakeUserContext { UserId = user.Id }, repo);
        await deleteHandler.Handle(new DeleteUserAddressCommand(nonDefault.Id), CancellationToken.None);

        var getHandler = new GetUserAddressesQueryHandler(new FakeUserContext { UserId = user.Id }, repo);
        var addresses = await getHandler.Handle(new GetUserAddressesQuery(), CancellationToken.None);
        addresses.Should().NotContain(a => a.Id == nonDefault.Id);
    }

    [Fact]
    public async Task Handle_WithDefaultAddress_ThrowsDomainException()
    {
        var user = NewUser("remove-default@hivespace.local");
        var defaultAddr = user.AddAddress("Name", "0900000000", "Main St", "Ward", "City", "VN", null, AddressType.Home, true);
        user.AddAddress("Name", "0900000000", "Other St", "Ward", "City", "VN", null, AddressType.Work);
        _fixture.DbContext.Users.Add(user);
        await _fixture.DbContext.SaveChangesAsync();

        var handler = new DeleteUserAddressCommandHandler(
            new FakeUserContext { UserId = user.Id },
            new SqlUserRepository(_fixture.DbContext));

        var act = () => handler.Handle(new DeleteUserAddressCommand(defaultAddr.Id), CancellationToken.None);

        await act.Should().ThrowAsync<CannotRemoveDefaultAddressException>();
    }

    [Fact]
    public async Task Handle_WithNonExistentUser_ThrowsNotFoundException()
    {
        var handler = new DeleteUserAddressCommandHandler(
            new FakeUserContext { UserId = Guid.NewGuid() },
            new SqlUserRepository(_fixture.DbContext));

        var act = () => handler.Handle(new DeleteUserAddressCommand(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    private static User NewUser(string email) =>
        User.CreateProfile(Guid.NewGuid(), Email.Create(email), email, "Address User");
}
