using FluentAssertions;
using HiveSpace.Domain.Shared.Enumerations;
using HiveSpace.NotificationService.Core.DomainModels.External;
using HiveSpace.NotificationService.Core.Persistence;
using HiveSpace.NotificationService.Core.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HiveSpace.NotificationService.Tests.Persistence.Repositories;

public class UserRefRepositoryTests
{
    [Fact]
    public async Task UpsertAsync_WhenUserDoesNotExist_InsertsUserRef()
    {
        await using var db = CreateDbContext();
        var repository = new UserRefRepository(db);
        var userId = Guid.NewGuid();

        await repository.UpsertAsync(UserRef.Create(userId, "user@test.com", "Test User"));

        var saved = await db.UserRefs.SingleAsync(u => u.Id == userId);
        saved.Email.Should().Be("user@test.com");
        saved.FullName.Should().Be("Test User");
    }

    [Fact]
    public async Task UpsertAsync_WhenUserExists_UpdatesUserRef()
    {
        await using var db = CreateDbContext();
        var repository = new UserRefRepository(db);
        var userId = Guid.NewGuid();

        await repository.UpsertAsync(UserRef.Create(userId, "old@test.com", "Old User"));
        await repository.UpsertAsync(UserRef.Create(
            userId,
            "new@test.com",
            "New User",
            phoneNumber: "+84999",
            locale: Culture.En,
            userName: "newuser",
            avatarUrl: "https://img/avatar.png"));

        var saved = await db.UserRefs.SingleAsync(u => u.Id == userId);
        saved.Email.Should().Be("new@test.com");
        saved.FullName.Should().Be("New User");
        saved.PhoneNumber.Should().Be("+84999");
        saved.Locale.Should().Be(Culture.En);
        saved.UserName.Should().Be("newuser");
        saved.AvatarUrl.Should().Be("https://img/avatar.png");
    }

    private static NotificationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<NotificationDbContext>()
            .UseInMemoryDatabase($"user-ref-repository-tests-{Guid.NewGuid()}")
            .Options;

        return new NotificationDbContext(options);
    }
}
