using FluentAssertions;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.Testing.Shared.Doubles;
using HiveSpace.UserService.Application.UserAddresses.Commands.UpdateUserAddress;
using HiveSpace.UserService.Application.UserAddresses.Dtos;
using HiveSpace.UserService.Application.UserAddresses.Queries.GetUserAddressById;
using HiveSpace.UserService.Application.UserAddresses.Queries.GetUserAddresses;
using HiveSpace.UserService.Domain.Aggregates.User;
using HiveSpace.UserService.Domain.Enums;
using HiveSpace.UserService.Infrastructure.Repositories;
using HiveSpace.UserService.Tests.Fixtures;
using Xunit;

namespace HiveSpace.UserService.Tests.Application.Addresses;

public class UpdateUserAddressCommandHandlerTests : IClassFixture<UserServiceFixture>
{
    private readonly UserServiceFixture _fixture;

    public UpdateUserAddressCommandHandlerTests(UserServiceFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Handle_WithNewStreet_ChangesStoredStreet()
    {
        var user = NewUser("update-addr@hivespace.local");
        var addr = user.AddAddress("Name", "0900000000", "Old St", "Ward", "City", "VN", null, AddressType.Home, true);
        _fixture.DbContext.Users.Add(user);
        await _fixture.DbContext.SaveChangesAsync();

        var repo = new SqlUserRepository(_fixture.DbContext);
        var updateHandler = new UpdateUserAddressCommandHandler(new FakeUserContext { UserId = user.Id }, repo);
        await updateHandler.Handle(
            new UpdateUserAddressCommand(addr.Id,
                new UserAddressRequestDto("Name", "0900000000", "New St", "Ward", "City", "VN", null, AddressType.Home, false)),
            CancellationToken.None);

        var getHandler = new GetUserAddressByIdQueryHandler(new FakeUserContext { UserId = user.Id }, repo);
        var result = await getHandler.Handle(new GetUserAddressByIdQuery(addr.Id), CancellationToken.None);
        result!.Street.Should().Be("New St");
    }

    [Fact]
    public async Task Handle_WithIsDefault_PromotesAddressToDefault()
    {
        var user = NewUser("update-addr-default@hivespace.local");
        var first = user.AddAddress("Name", "0900000000", "St 1", "Ward", "City", "VN", null, AddressType.Home, true);
        var second = user.AddAddress("Name", "0900000000", "St 2", "Ward", "City", "VN", null, AddressType.Work);
        _fixture.DbContext.Users.Add(user);
        await _fixture.DbContext.SaveChangesAsync();

        var repo = new SqlUserRepository(_fixture.DbContext);
        var updateHandler = new UpdateUserAddressCommandHandler(new FakeUserContext { UserId = user.Id }, repo);
        await updateHandler.Handle(
            new UpdateUserAddressCommand(second.Id,
                new UserAddressRequestDto("Name", "0900000000", "St 2", "Ward", "City", "VN", null, AddressType.Work, IsDefault: true)),
            CancellationToken.None);

        var getHandler = new GetUserAddressesQueryHandler(new FakeUserContext { UserId = user.Id }, repo);
        var addresses = await getHandler.Handle(new GetUserAddressesQuery(), CancellationToken.None);
        addresses.Single(a => a.Id == second.Id).IsDefault.Should().BeTrue();
        addresses.Single(a => a.Id == first.Id).IsDefault.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WithNonExistentUser_ThrowsNotFoundException()
    {
        var handler = new UpdateUserAddressCommandHandler(
            new FakeUserContext { UserId = Guid.NewGuid() },
            new SqlUserRepository(_fixture.DbContext));

        var act = () => handler.Handle(
            new UpdateUserAddressCommand(Guid.NewGuid(),
                new UserAddressRequestDto("Name", "15551234567", "Street", "Ward", "City", "VN", null, AddressType.Home, false)),
            CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    private static User NewUser(string email) =>
        User.CreateProfile(Guid.NewGuid(), Email.Create(email), email, "Update User");
}
