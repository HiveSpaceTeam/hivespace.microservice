using FluentAssertions;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.Testing.Shared.Doubles;
using HiveSpace.UserService.Application.Interfaces.Messaging;
using HiveSpace.UserService.Application.Stores.Commands.CreateStore;
using HiveSpace.UserService.Application.Stores.Dtos;
using HiveSpace.UserService.Domain.Aggregates.Store;
using HiveSpace.UserService.Domain.Aggregates.User;
using HiveSpace.UserService.Domain.Exceptions;
using HiveSpace.UserService.Domain.Repositories;
using HiveSpace.UserService.Domain.Services;
using HiveSpace.UserService.Infrastructure.Repositories;
using HiveSpace.UserService.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HiveSpace.UserService.Tests.Application.StoreOnboarding;

public class CreateStoreCommandHandlerTests : IClassFixture<UserServiceFixture>
{
    private readonly UserServiceFixture _fixture;

    public CreateStoreCommandHandlerTests(UserServiceFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Handle_WithEligibleUser_PersistsStoreInDatabase()
    {
        var user = NewUser("store-submit@hivespace.local");
        _fixture.DbContext.Users.Add(user);
        await _fixture.DbContext.SaveChangesAsync();

        var handler = CreateHandler(user.Id);

        var result = await handler.Handle(
            new CreateStoreCommand(new CreateStoreRequestDto("Submit Store", "desc", "logo-submit", "123 Seller St")),
            CancellationToken.None);

        var stored = await _fixture.DbContext.Stores.SingleAsync(s => s.Id == result.StoreId);
        stored.StoreName.Should().Be("Submit Store");
        stored.OwnerId.Should().Be(user.Id);
    }

    [Fact]
    public async Task Handle_WithNonExistentUser_ThrowsNotFoundException()
    {
        var handler = CreateHandler(Guid.NewGuid());

        var act = () => handler.Handle(
            new CreateStoreCommand(new CreateStoreRequestDto("Missing Owner Store", null, "logo-2", "123 Seller St")),
            CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WithDuplicateStore_ThrowsUserStoreExistsException()
    {
        var user = NewUser("store-duplicate@hivespace.local");
        _fixture.DbContext.Users.Add(user);
        await _fixture.DbContext.SaveChangesAsync();

        await CreateHandler(user.Id).Handle(
            new CreateStoreCommand(new CreateStoreRequestDto("First Store", null, "logo-a", "123 Seller St")),
            CancellationToken.None);

        var act = () => CreateHandler(user.Id).Handle(
            new CreateStoreCommand(new CreateStoreRequestDto("Second Store", null, "logo-b", "123 Seller St")),
            CancellationToken.None);

        await act.Should().ThrowAsync<UserStoreExistsException>();
    }

    private static User NewUser(string email) =>
        User.CreateProfile(Guid.NewGuid(), Email.Create(email), email, "Store Owner");

    private CreateStoreCommandHandler CreateHandler(Guid userId)
    {
        IUserRepository userRepository = new SqlUserRepository(_fixture.DbContext);
        IStoreRepository storeRepository = new SqlStoreRepository(_fixture.DbContext);
        var storeManager = new StoreManager(storeRepository, userRepository);

        return new CreateStoreCommandHandler(
            new FakeUserContext { UserId = userId },
            storeManager,
            storeRepository,
            new StoreEventPublisherFake());
    }

    private sealed class StoreEventPublisherFake : IStoreEventPublisher
    {
        public Task PublishStoreCreatedAsync(Store store, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task PublishStoreUpdatedAsync(Store store, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
