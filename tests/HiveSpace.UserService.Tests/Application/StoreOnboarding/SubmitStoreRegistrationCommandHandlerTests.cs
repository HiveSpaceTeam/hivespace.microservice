using FluentAssertions;
using HiveSpace.Testing.Shared.Doubles;
using HiveSpace.UserService.Application.Interfaces.Messaging;
using HiveSpace.UserService.Application.Stores.Commands.CreateStore;
using HiveSpace.UserService.Application.Stores.Dtos;
using HiveSpace.UserService.Domain.Aggregates.Store;
using HiveSpace.UserService.Domain.Aggregates.User;
using HiveSpace.UserService.Domain.Repositories;
using HiveSpace.UserService.Domain.Services;
using HiveSpace.UserService.Infrastructure.Repositories;
using HiveSpace.UserService.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HiveSpace.UserService.Tests.Application.StoreOnboarding;

public class SubmitStoreRegistrationCommandHandlerTests : IClassFixture<UserServiceFixture>
{
    private readonly UserServiceFixture _fixture;

    public SubmitStoreRegistrationCommandHandlerTests(UserServiceFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Handle_CreatesPendingStoreProfile()
    {
        var user = NewUser("submit-store@hivespace.local");
        _fixture.DbContext.Users.Add(user);
        await _fixture.DbContext.SaveChangesAsync();

        var result = await CreateHandler(user.Id).Handle(
            new CreateStoreCommand(new CreateStoreRequestDto("My Store", "desc", "logo-1", "1 Main St")),
            CancellationToken.None);

        var stored = await _fixture.DbContext.Stores.SingleAsync(s => s.Id == result.StoreId);
        stored.StoreName.Should().Be("My Store");
        stored.OwnerId.Should().Be(user.Id);
    }

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

    private static User NewUser(string email) =>
        User.CreateProfile(Guid.NewGuid(), Email.Create(email), email, "Seller");

    private sealed class StoreEventPublisherFake : IStoreEventPublisher
    {
        public Task PublishStoreCreatedAsync(Store store, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task PublishStoreUpdatedAsync(Store store, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
