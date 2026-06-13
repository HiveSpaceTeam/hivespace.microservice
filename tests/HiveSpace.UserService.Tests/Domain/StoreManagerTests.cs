using FluentAssertions;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.Domain.Shared.Interfaces;
using HiveSpace.Domain.Shared.Specifications;
using HiveSpace.UserService.Domain.Aggregates.Store;
using HiveSpace.UserService.Domain.Aggregates.User;
using HiveSpace.UserService.Domain.Exceptions;
using HiveSpace.UserService.Domain.Repositories;
using HiveSpace.UserService.Domain.Services;
using Xunit;

namespace HiveSpace.UserService.Tests.Domain;

public class StoreManagerTests
{
    [Fact]
    public async Task RegisterStore_WhenUserAlreadyOwnsStore_ThrowsUserStoreExistsException()
    {
        var userId = Guid.NewGuid();
        var user = User.CreateProfile(userId, Email.Create("owner@example.com"), "storeowner", "Store Owner");
        var storeRepo = new StubStoreRepository(ownerHasStore: true);
        var userRepo = new StubUserRepository(user);
        var manager = new StoreManager(storeRepo, userRepo);

        var act = () => manager.RegisterStoreAsync("My Store", null, "logo", "addr", userId);

        await act.Should().ThrowAsync<UserStoreExistsException>();
    }

    [Fact]
    public async Task RegisterStore_WithDuplicateStoreName_ThrowsConflictException()
    {
        var userId = Guid.NewGuid();
        var user = User.CreateProfile(userId, Email.Create("dup@example.com"), "dupuser", "Dup User");
        var storeRepo = new StubStoreRepository(ownerHasStore: false, nameExists: true);
        var userRepo = new StubUserRepository(user);
        var manager = new StoreManager(storeRepo, userRepo);

        var act = () => manager.RegisterStoreAsync("Duplicate Store", null, "logo", "addr", userId);

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task RegisterStore_WithForbiddenCharactersInName_ThrowsInvalidStoreInformationException()
    {
        var storeRepo = new StubStoreRepository();
        var userRepo = new StubUserRepository(null);
        var manager = new StoreManager(storeRepo, userRepo);

        var act = () => manager.RegisterStoreAsync("Bad<Name>", null, "logo", "addr", Guid.NewGuid());

        await act.Should().ThrowAsync<InvalidStoreInformationException>();
    }

    private sealed class StubStoreRepository(bool ownerHasStore = false, bool nameExists = false) : IStoreRepository
    {
        public Task<Store?> GetByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken = default)
            => Task.FromResult(ownerHasStore ? Store.Create("Existing Store", null, "logo", "addr", ownerId, null) : (Store?)null);

        public Task<bool> StoreNameExistsAsync(string storeName, CancellationToken cancellationToken = default)
            => Task.FromResult(nameExists);

        public IQueryable<Store> GetQueryable() => throw new NotImplementedException();
        public Task<List<Store>> GetListAsync(CancellationToken cancellationToken = default) => Task.FromResult(new List<Store>());
        public Task<List<Store>> GetListAsync(Specification<Store> specification, CancellationToken cancellationToken = default) => Task.FromResult(new List<Store>());
        public Task<int> GetCountAsync(Specification<Store> specification, CancellationToken cancellationToken = default) => Task.FromResult(0);
        public Task<List<Store>> GetPagedAsync(Specification<Store> specification, int page, int pageSize, CancellationToken cancellationToken = default) => Task.FromResult(new List<Store>());
        public Task<Store?> GetByIdAsync(object id, bool includeDetail = false, CancellationToken cancellationToken = default) => Task.FromResult((Store?)null);
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(0);
        public void Add(Store entity) { }
        public void Remove(Store entity) { }
        void IRepository<Store>.UpdateWithConcurrency<T>(T entity, DateTimeOffset originalDateTimeUpdated, DateTimeOffset newDateTimeUpdated) { }
        void IRepository<Store>.UpdateWithConcurrency<T>(T entity, DateTimeOffset originalDateTimeUpdated) { }
    }

    private sealed class StubUserRepository(User? user) : IUserRepository
    {
        public Task<User?> GetByIdAsync(Guid id, bool includeDetail = false, CancellationToken cancellationToken = default, bool asTracking = false)
            => Task.FromResult(user);

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(0);
        public Task<List<User>> GetAllAsync(CancellationToken cancellationToken = default) => Task.FromResult(new List<User>());
        public Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default) => Task.FromResult((User?)null);
        public Task<User?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default) => Task.FromResult((User?)null);
        public Task<User> CreateUserAsync(User domainUser, string password, CancellationToken cancellationToken = default) => Task.FromResult(domainUser);
        public Task<User> RemoveUserAsync(Guid userId, CancellationToken cancellationToken = default) => Task.FromResult(user!);
    }
}
