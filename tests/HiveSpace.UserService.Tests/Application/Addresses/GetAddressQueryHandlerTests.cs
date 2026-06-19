using FluentAssertions;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.Testing.Shared.Doubles;
using HiveSpace.UserService.Application.UserAddresses.Queries.GetDefaultUserAddress;
using HiveSpace.UserService.Application.UserAddresses.Queries.GetUserAddressById;
using HiveSpace.UserService.Application.UserAddresses.Queries.GetUserAddresses;
using HiveSpace.UserService.Domain.Aggregates.User;
using HiveSpace.UserService.Domain.Enums;
using HiveSpace.UserService.Infrastructure.Repositories;
using HiveSpace.UserService.Tests.Fixtures;
using Xunit;

namespace HiveSpace.UserService.Tests.Application.Addresses;

public class GetAddressQueryHandlerTests : IClassFixture<UserServiceFixture>
{
    private readonly UserServiceFixture _fixture;

    public GetAddressQueryHandlerTests(UserServiceFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task GetById_WithExistingId_ReturnsMatchingDto()
    {
        var user = NewUser("getbyid@hivespace.local");
        var addr = user.AddAddress("John Doe", "0900000000", "Test St", "Ward 1", "Hanoi", "VN", null, AddressType.Home, true);
        _fixture.DbContext.Users.Add(user);
        await _fixture.DbContext.SaveChangesAsync();

        var handler = new GetUserAddressByIdQueryHandler(
            new FakeUserContext { UserId = user.Id },
            new SqlUserRepository(_fixture.DbContext));

        var result = await handler.Handle(new GetUserAddressByIdQuery(addr.Id), CancellationToken.None);

        result.Should().NotBeNull();
        result!.Street.Should().Be("Test St");
        result.Id.Should().Be(addr.Id);
    }

    [Fact]
    public async Task GetById_WithNonExistentId_ReturnsNull()
    {
        var user = NewUser("getbyid-null@hivespace.local");
        user.AddAddress("Name", "0900000000", "Street", "Ward", "City", "VN", null, AddressType.Home, true);
        _fixture.DbContext.Users.Add(user);
        await _fixture.DbContext.SaveChangesAsync();

        var handler = new GetUserAddressByIdQueryHandler(
            new FakeUserContext { UserId = user.Id },
            new SqlUserRepository(_fixture.DbContext));

        var result = await handler.Handle(new GetUserAddressByIdQuery(Guid.NewGuid()), CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetDefault_WithDefaultAddress_ReturnsDefaultDto()
    {
        var user = NewUser("getdefault@hivespace.local");
        var defaultAddr = user.AddAddress("Name", "0900000000", "Default St", "Ward", "City", "VN", null, AddressType.Home, true);
        user.AddAddress("Name", "0900000000", "Other St", "Ward", "City", "VN", null, AddressType.Work);
        _fixture.DbContext.Users.Add(user);
        await _fixture.DbContext.SaveChangesAsync();

        var handler = new GetDefaultUserAddressQueryHandler(
            new FakeUserContext { UserId = user.Id },
            new SqlUserRepository(_fixture.DbContext));

        var result = await handler.Handle(new GetDefaultUserAddressQuery(), CancellationToken.None);

        result.Should().NotBeNull();
        result!.Id.Should().Be(defaultAddr.Id);
        result.IsDefault.Should().BeTrue();
    }

    [Fact]
    public async Task GetDefault_WithNoDefaultAddress_ReturnsNull()
    {
        var user = NewUser("getdefault-none@hivespace.local");
        user.AddAddress("Name", "0900000000", "Street", "Ward", "City", "VN", null, AddressType.Home);
        _fixture.DbContext.Users.Add(user);
        await _fixture.DbContext.SaveChangesAsync();

        var handler = new GetDefaultUserAddressQueryHandler(
            new FakeUserContext { UserId = user.Id },
            new SqlUserRepository(_fixture.DbContext));

        var result = await handler.Handle(new GetDefaultUserAddressQuery(), CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAll_ReturnsAllAddressesForUser()
    {
        var user = NewUser("getall@hivespace.local");
        user.AddAddress("Name", "0900000000", "St 1", "Ward", "City", "VN", null, AddressType.Home, true);
        user.AddAddress("Name", "0900000000", "St 2", "Ward", "City", "VN", null, AddressType.Work);
        _fixture.DbContext.Users.Add(user);
        await _fixture.DbContext.SaveChangesAsync();

        var handler = new GetUserAddressesQueryHandler(
            new FakeUserContext { UserId = user.Id },
            new SqlUserRepository(_fixture.DbContext));

        var result = await handler.Handle(new GetUserAddressesQuery(), CancellationToken.None);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetById_WithNonExistentUser_ThrowsNotFoundException()
    {
        var handler = new GetUserAddressByIdQueryHandler(
            new FakeUserContext { UserId = Guid.NewGuid() },
            new SqlUserRepository(_fixture.DbContext));

        var act = () => handler.Handle(new GetUserAddressByIdQuery(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetDefault_WithNonExistentUser_ThrowsNotFoundException()
    {
        var handler = new GetDefaultUserAddressQueryHandler(
            new FakeUserContext { UserId = Guid.NewGuid() },
            new SqlUserRepository(_fixture.DbContext));

        var act = () => handler.Handle(new GetDefaultUserAddressQuery(), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetAll_WithNonExistentUser_ThrowsNotFoundException()
    {
        var handler = new GetUserAddressesQueryHandler(
            new FakeUserContext { UserId = Guid.NewGuid() },
            new SqlUserRepository(_fixture.DbContext));

        var act = () => handler.Handle(new GetUserAddressesQuery(), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    private static User NewUser(string email) =>
        User.CreateProfile(Guid.NewGuid(), Email.Create(email), email, "Query User");
}
