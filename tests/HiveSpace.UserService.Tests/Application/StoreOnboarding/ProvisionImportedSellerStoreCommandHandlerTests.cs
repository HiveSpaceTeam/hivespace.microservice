using FluentAssertions;
using HiveSpace.UserService.Application.Interfaces.Messaging;
using HiveSpace.UserService.Application.Stores.Commands.ProvisionImportedSellerStore;
using HiveSpace.UserService.Domain.Aggregates.Store;
using HiveSpace.UserService.Domain.Aggregates.User;
using HiveSpace.UserService.Domain.Repositories;
using HiveSpace.UserService.Domain.Services;
using HiveSpace.UserService.Infrastructure.Repositories;
using HiveSpace.UserService.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HiveSpace.UserService.Tests.Application.StoreOnboarding;

public class ProvisionImportedSellerStoreCommandHandlerTests : IClassFixture<UserServiceFixture>
{
    private readonly UserServiceFixture _fixture;

    public ProvisionImportedSellerStoreCommandHandlerTests(UserServiceFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Handle_WithNewExternalSellerStore_CreatesStoreForUser()
    {
        var userId = Guid.NewGuid();
        var publisher = new StoreEventPublisherFake();

        var result = await CreateHandler(publisher).Handle(
            new ProvisionImportedSellerStoreCommand(
                "tiki",
                "seller-store-new",
                userId,
                "Imported Seller",
                "https://tiki.vn/seller-store-new",
                "https://cdn.example.com/sellers/imported-seller.png"),
            CancellationToken.None);

        result.Status.Should().Be("Created");
        result.StoreId.Should().NotBeNull();
        var stored = await _fixture.DbContext.Stores.SingleAsync(s => s.Id == result.StoreId);
        stored.OwnerId.Should().Be(userId);
        stored.StoreName.Should().Be("Imported Seller");
        stored.LogoFileId.Should().Be("imported-seller-placeholder-logo");
        stored.LogoUrl.Should().Be("https://cdn.example.com/sellers/imported-seller.png");
        publisher.CreatedStores.Should().ContainSingle();
    }

    [Fact]
    public async Task Handle_WithExistingOwnership_ReturnsMatchedStoreId()
    {
        var user = User.CreateProfile(Guid.NewGuid(), Email.Create("existing-store@hivespace.local"), "existing-store", "Existing Store");
        _fixture.DbContext.Users.Add(user);
        await _fixture.DbContext.SaveChangesAsync();

        var first = await CreateHandler().Handle(
            new ProvisionImportedSellerStoreCommand("tiki", "seller-store-existing", user.Id, "Existing Imported Store", null),
            CancellationToken.None);

        var second = await CreateHandler().Handle(
            new ProvisionImportedSellerStoreCommand("tiki", "seller-store-existing", user.Id, "Existing Imported Store", null),
            CancellationToken.None);

        second.Status.Should().Be("Matched");
        second.StoreId.Should().Be(first.StoreId);
    }

    [Fact]
    public async Task Handle_WithStoreUniquenessConflict_ReturnsConflictStatus()
    {
        var existingUser = User.CreateProfile(Guid.NewGuid(), Email.Create("owner-conflict@hivespace.local"), "owner-conflict", "Owner Conflict");
        _fixture.DbContext.Users.Add(existingUser);
        await _fixture.DbContext.SaveChangesAsync();

        await CreateHandler().Handle(
            new ProvisionImportedSellerStoreCommand("tiki", "seller-a", existingUser.Id, "Shared Store", null),
            CancellationToken.None);

        var result = await CreateHandler().Handle(
            new ProvisionImportedSellerStoreCommand("tiki", "seller-b", Guid.NewGuid(), "Shared Store", null),
            CancellationToken.None);

        result.Status.Should().Be("Conflict");
        result.ConflictReason.Should().Be("StoreNameAlreadyExists");
    }

    [Theory]
    [InlineData("Tiki Trading Official", "Tiki Trading", true)]
    [InlineData("Tiki Trade Official", "Tiki Trade", false)]
    [InlineData("Acme Trading Store", "Store Acme Trading", false)]
    [InlineData("Acme Official Store", "Acme Official Store Vietnam", true)]
    [InlineData("Acme Official Store Vietnam", "Acme Official Store", true)]
    [InlineData("Café-Official Store", "CAFE Official  Store Vietnam", true)]
    public async Task Handle_WithSimilarExistingStoreName_ReturnsConflictStatus(
        string existingName, string incomingName, bool expectsConflict)
    {
        var caseId = Guid.NewGuid().ToString("N");
        var existingUser = User.CreateProfile(Guid.NewGuid(), Email.Create($"{caseId}@hivespace.local"), caseId, "Similar Owner");
        _fixture.DbContext.Users.Add(existingUser);
        await _fixture.DbContext.SaveChangesAsync();

        var storeManager = new StoreManager(new SqlStoreRepository(_fixture.DbContext), new SqlUserRepository(_fixture.DbContext));
        var registration = await storeManager.RegisterStoreAsync(existingName, null, "test-logo", "test-address", existingUser.Id);
        var existingStore = registration.Store;
        _fixture.DbContext.Stores.Add(existingStore);
        await _fixture.DbContext.SaveChangesAsync();

        try
        {
            var result = await CreateHandler().Handle(
                new ProvisionImportedSellerStoreCommand("tiki", caseId, Guid.NewGuid(), incomingName, null),
                CancellationToken.None);

            result.Status.Should().Be(expectsConflict ? "Conflict" : "Created");
            result.ConflictReason.Should().Be(expectsConflict ? "SimilarStoreNameConflict" : null);
            if (!expectsConflict)
            {
                var createdStore = await _fixture.DbContext.Stores.SingleAsync(s => s.Id == result.StoreId);
                createdStore.StoreName.Should().Be(incomingName);
                _fixture.DbContext.Stores.Remove(createdStore);
            }
        }
        finally
        {
            _fixture.DbContext.Stores.Remove(existingStore);
            await _fixture.DbContext.SaveChangesAsync();
        }
    }

    [Fact]
    public async Task Handle_WithCreatedStore_PublishesStoreCreatedIntegrationEvent()
    {
        var publisher = new StoreEventPublisherFake();

        await CreateHandler(publisher).Handle(
            new ProvisionImportedSellerStoreCommand("tiki", "seller-publish", Guid.NewGuid(), "Publish Store", null),
            CancellationToken.None);

        publisher.CreatedStores.Should().ContainSingle(x => x.StoreName == "Publish Store");
    }

    private ProvisionImportedSellerStoreCommandHandler CreateHandler(StoreEventPublisherFake? publisher = null)
    {
        IUserRepository userRepository = new SqlUserRepository(_fixture.DbContext);
        IStoreRepository storeRepository = new SqlStoreRepository(_fixture.DbContext);
        var storeManager = new StoreManager(storeRepository, userRepository);

        return new ProvisionImportedSellerStoreCommandHandler(
            userRepository,
            storeRepository,
            storeManager,
            publisher ?? new StoreEventPublisherFake(),
            new UserEventPublisherFake());
    }

    private sealed class StoreEventPublisherFake : IStoreEventPublisher
    {
        public List<Store> CreatedStores { get; } = [];

        public Task PublishStoreCreatedAsync(Store store, CancellationToken cancellationToken = default)
        {
            CreatedStores.Add(store);
            return Task.CompletedTask;
        }

        public Task PublishStoreUpdatedAsync(Store store, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class UserEventPublisherFake : IUserEventPublisher
    {
        public Task PublishUserCreatedAsync(User user, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task PublishUserUpdatedAsync(User user, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
