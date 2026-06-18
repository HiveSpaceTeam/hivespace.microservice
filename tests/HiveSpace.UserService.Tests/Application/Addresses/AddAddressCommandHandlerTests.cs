using FluentAssertions;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.Testing.Shared.Doubles;
using HiveSpace.UserService.Application.UserAddresses.Commands.CreateUserAddress;
using HiveSpace.UserService.Application.UserAddresses.Dtos;
using HiveSpace.UserService.Domain.Aggregates.User;
using HiveSpace.UserService.Domain.Enums;
using HiveSpace.UserService.Domain.Exceptions;
using HiveSpace.UserService.Domain.Repositories;
using Xunit;

namespace HiveSpace.UserService.Tests.Application.Addresses;

public class CreateUserAddressCommandHandlerTests
{
    // EF InMemory cannot insert a new Address into a User that was seeded without one —
    // the DbUpdateConcurrencyException is a known InMemory limitation with owned entities.
    // Use a stub repository to test the handler logic without EF persistence.

    [Fact]
    public async Task Handle_WithValidAddress_ReturnsCreatedAddressDto()
    {
        var user = NewUser("add-address@hivespace.local");
        var handler = new CreateUserAddressCommandHandler(
            new FakeUserContext { UserId = user.Id },
            new StubUserRepository(user));

        var result = await handler.Handle(
            new CreateUserAddressCommand(new UserAddressRequestDto(
                "John Doe", "0901234567", "123 Main St", "Ward 1", "Hanoi", "VN", null, AddressType.Home, true)),
            CancellationToken.None);

        result.Street.Should().Be("123 Main St");
        result.IsDefault.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithPhoneNumberExceedingMaxLength_ThrowsDomainException()
    {
        var user = NewUser("add-address-invalid@hivespace.local");
        var handler = new CreateUserAddressCommandHandler(
            new FakeUserContext { UserId = user.Id },
            new StubUserRepository(user));

        var longPhone = new string('0', 21);
        var act = () => handler.Handle(
            new CreateUserAddressCommand(new UserAddressRequestDto(
                "Name", longPhone, "Street", "Ward", "Hanoi", "VN", null, AddressType.Home, false)),
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidAddressException>();
    }

    [Fact]
    public async Task Handle_WithNonExistentUser_ThrowsNotFoundException()
    {
        var user = NewUser("add-address-notfound@hivespace.local");
        var handler = new CreateUserAddressCommandHandler(
            new FakeUserContext { UserId = Guid.NewGuid() },
            new StubUserRepository(user));

        var act = () => handler.Handle(
            new CreateUserAddressCommand(new UserAddressRequestDto(
                "Name", "15551234567", "Street", "Ward", "City", "VN", null, AddressType.Home, false)),
            CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    private static User NewUser(string email) =>
        User.CreateProfile(Guid.NewGuid(), Email.Create(email), email, "Address User");

    private sealed class StubUserRepository(User user) : IUserRepository
    {
        public Task<User?> GetByIdAsync(Guid id, bool includeDetail = false, CancellationToken cancellationToken = default, bool asTracking = false)
            => Task.FromResult<User?>(user.Id == id ? user : null);

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(1);

        public Task<List<User>> GetAllAsync(CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<User?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<User> CreateUserAsync(User domainUser, string password, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<User> RemoveUserAsync(Guid userId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }
}
