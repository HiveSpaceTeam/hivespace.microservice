using FluentAssertions;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.Infrastructure.Messaging.Shared.Events.Users;
using HiveSpace.UserService.Api.Consumers;
using HiveSpace.UserService.Domain.Aggregates.User;
using HiveSpace.UserService.Infrastructure.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Xunit;

namespace HiveSpace.UserService.Tests.Consumers;

public class IdentityUserReadyConsumerTests
{
    private UserDbContext CreateDb() =>
        new(new DbContextOptionsBuilder<UserDbContext>()
            .UseInMemoryDatabase($"user-identity-{Guid.NewGuid()}")
            .Options);

    [Fact]
    public async Task Consume_WhenUserIdEmpty_ThrowsInvalidFieldException()
    {
        var db = CreateDb();
        var consumer = new IdentityUserReadyConsumer(db);
        var msg = new IdentityUserReadyIntegrationEvent { UserId = Guid.Empty, Email = "test@example.com" };
        var ctx = Substitute.For<ConsumeContext<IdentityUserReadyIntegrationEvent>>();
        ctx.Message.Returns(msg);
        ctx.CancellationToken.Returns(CancellationToken.None);

        var act = async () => await consumer.Consume(ctx);

        await act.Should().ThrowAsync<InvalidFieldException>();
    }

    [Fact]
    public async Task Consume_WhenEmailEmpty_ThrowsInvalidFieldException()
    {
        var db = CreateDb();
        var consumer = new IdentityUserReadyConsumer(db);
        var msg = new IdentityUserReadyIntegrationEvent { UserId = Guid.NewGuid(), Email = "  " };
        var ctx = Substitute.For<ConsumeContext<IdentityUserReadyIntegrationEvent>>();
        ctx.Message.Returns(msg);
        ctx.CancellationToken.Returns(CancellationToken.None);

        var act = async () => await consumer.Consume(ctx);

        await act.Should().ThrowAsync<InvalidFieldException>();
    }

    [Fact]
    public async Task Consume_WhenUserAlreadyExists_DoesNotCreateDuplicate()
    {
        var db = CreateDb();
        var userId = Guid.NewGuid();
        var existing = User.CreateProfile(userId, Email.Create("existing@example.com"), "existinguser", "Existing User");
        db.Users.Add(existing);
        await db.SaveChangesAsync();

        var consumer = new IdentityUserReadyConsumer(db);
        var msg = new IdentityUserReadyIntegrationEvent { UserId = userId, Email = "existing@example.com", UserName = "existinguser", FullName = "Existing User" };
        var ctx = Substitute.For<ConsumeContext<IdentityUserReadyIntegrationEvent>>();
        ctx.Message.Returns(msg);
        ctx.CancellationToken.Returns(CancellationToken.None);

        await consumer.Consume(ctx);

        db.Users.Count().Should().Be(1);
    }

    [Fact]
    public async Task Consume_WhenNewUser_CreatesUserProfile()
    {
        var db = CreateDb();
        var consumer = new IdentityUserReadyConsumer(db);
        var userId = Guid.NewGuid();
        var msg = new IdentityUserReadyIntegrationEvent { UserId = userId, Email = "newuser@example.com", UserName = "newuser", FullName = "New User" };
        var ctx = Substitute.For<ConsumeContext<IdentityUserReadyIntegrationEvent>>();
        ctx.Message.Returns(msg);
        ctx.CancellationToken.Returns(CancellationToken.None);

        await consumer.Consume(ctx);

        db.Users.Should().ContainSingle(u => u.Id == userId);
    }

    [Fact]
    public async Task Consume_WhenUserNameIsNull_UsesEmailAsUserName()
    {
        var db = CreateDb();
        var consumer = new IdentityUserReadyConsumer(db);
        var userId = Guid.NewGuid();
        var msg = new IdentityUserReadyIntegrationEvent { UserId = userId, Email = "fallback@example.com", UserName = null, FullName = "Test User" };
        var ctx = Substitute.For<ConsumeContext<IdentityUserReadyIntegrationEvent>>();
        ctx.Message.Returns(msg);
        ctx.CancellationToken.Returns(CancellationToken.None);

        await consumer.Consume(ctx);

        db.Users.Should().ContainSingle(u => u.Id == userId);
    }

    [Fact]
    public async Task Consume_WhenFullNameIsNull_UsesUserNameAsFullName()
    {
        var db = CreateDb();
        var consumer = new IdentityUserReadyConsumer(db);
        var userId = Guid.NewGuid();
        var msg = new IdentityUserReadyIntegrationEvent { UserId = userId, Email = "user@example.com", UserName = "testuser", FullName = null };
        var ctx = Substitute.For<ConsumeContext<IdentityUserReadyIntegrationEvent>>();
        ctx.Message.Returns(msg);
        ctx.CancellationToken.Returns(CancellationToken.None);

        await consumer.Consume(ctx);

        db.Users.Should().ContainSingle(u => u.Id == userId);
    }

    [Fact]
    public async Task Consume_WhenReadyAtIsSet_UsesProvidedTimestampAsCreatedAt()
    {
        var db = CreateDb();
        var consumer = new IdentityUserReadyConsumer(db);
        var userId = Guid.NewGuid();
        var readyAt = new DateTime(2025, 3, 10, 8, 0, 0);
        var msg = new IdentityUserReadyIntegrationEvent
        {
            UserId = userId,
            Email = "readyat@example.com",
            UserName = "readyuser",
            FullName = "Ready User",
            ReadyAt = readyAt
        };
        var ctx = Substitute.For<ConsumeContext<IdentityUserReadyIntegrationEvent>>();
        ctx.Message.Returns(msg);
        ctx.CancellationToken.Returns(CancellationToken.None);

        await consumer.Consume(ctx);

        db.Users.Should().ContainSingle(u => u.Id == userId);
    }
}
