using HiveSpace.Domain.Shared.Enumerations;
using HiveSpace.Infrastructure.Messaging.Shared.Events.Users;
using HiveSpace.NotificationService.Api.Consumers.Sync;
using HiveSpace.NotificationService.Core.DomainModels.External;
using HiveSpace.NotificationService.Core.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace HiveSpace.NotificationService.Tests.Consumers.Sync;

public class UserSyncConsumerTests
{
    private readonly IUserRefRepository _userRefs = Substitute.For<IUserRefRepository>();
    private readonly ILogger<UserSyncConsumer> _logger = Substitute.For<ILogger<UserSyncConsumer>>();
    private readonly UserSyncConsumer _consumer;

    public UserSyncConsumerTests()
    {
        _consumer = new UserSyncConsumer(_userRefs, _logger);
    }

    [Fact]
    public async Task Consume_UserCreated_UpsertsNewUserRef()
    {
        var userId = Guid.NewGuid();
        var msg = new UserCreatedIntegrationEvent { UserId = userId, Email = "new@example.com", FullName = "New User", Locale = Culture.Vi };
        var ctx = Substitute.For<ConsumeContext<UserCreatedIntegrationEvent>>();
        ctx.Message.Returns(msg);
        ctx.CancellationToken.Returns(CancellationToken.None);

        await _consumer.Consume(ctx);

        await _userRefs.Received(1).UpsertAsync(Arg.Is<UserRef>(u => u.Id == userId), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Consume_UserUpdated_WhenExistingUserFound_UpdatesAndUpserts()
    {
        var userId = Guid.NewGuid();
        var existing = UserRef.Create(userId, "old@example.com", "Old Name");
        var msg = new UserUpdatedIntegrationEvent { UserId = userId, Email = "new@example.com", FullName = "New Name", Locale = Culture.Vi };
        var ctx = Substitute.For<ConsumeContext<UserUpdatedIntegrationEvent>>();
        ctx.Message.Returns(msg);
        ctx.CancellationToken.Returns(CancellationToken.None);
        _userRefs.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(existing);

        await _consumer.Consume(ctx);

        await _userRefs.Received(1).UpsertAsync(Arg.Is<UserRef>(u => u.Email == "new@example.com"), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Consume_UserUpdated_WhenUserNotFound_CreatesAndUpserts()
    {
        var userId = Guid.NewGuid();
        var msg = new UserUpdatedIntegrationEvent { UserId = userId, Email = "brand-new@example.com", FullName = "New User", Locale = Culture.Vi };
        var ctx = Substitute.For<ConsumeContext<UserUpdatedIntegrationEvent>>();
        ctx.Message.Returns(msg);
        ctx.CancellationToken.Returns(CancellationToken.None);
        _userRefs.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns((UserRef?)null);

        await _consumer.Consume(ctx);

        await _userRefs.Received(1).UpsertAsync(Arg.Is<UserRef>(u => u.Id == userId), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Consume_IdentityUserReady_WhenUserNotFound_CreatesAndUpserts()
    {
        var userId = Guid.NewGuid();
        var msg = new IdentityUserReadyIntegrationEvent
        {
            UserId = userId,
            Email = "imported@example.com",
            FullName = "Imported Seller",
            UserName = "imported-seller"
        };
        var ctx = Substitute.For<ConsumeContext<IdentityUserReadyIntegrationEvent>>();
        ctx.Message.Returns(msg);
        ctx.CancellationToken.Returns(CancellationToken.None);
        _userRefs.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns((UserRef?)null);

        await _consumer.Consume(ctx);

        await _userRefs.Received(1).UpsertAsync(
            Arg.Is<UserRef>(u =>
                u.Id == userId
                && u.Email == "imported@example.com"
                && u.FullName == "Imported Seller"
                && u.UserName == "imported-seller"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Consume_IdentityUserReady_WhenUserExists_PreservesExistingProfileFields()
    {
        var userId = Guid.NewGuid();
        var existing = UserRef.Create(
            userId,
            "old@example.com",
            "Old Name",
            phoneNumber: "0123456789",
            locale: Culture.En,
            userName: "old-user",
            avatarUrl: "https://cdn.example.com/avatar.png");
        existing.UpdateStore(Guid.NewGuid(), "Existing Store", "https://cdn.example.com/store.png");

        var msg = new IdentityUserReadyIntegrationEvent
        {
            UserId = userId,
            Email = "new@example.com",
            FullName = "New Imported Seller",
            UserName = "new-user"
        };
        var ctx = Substitute.For<ConsumeContext<IdentityUserReadyIntegrationEvent>>();
        ctx.Message.Returns(msg);
        ctx.CancellationToken.Returns(CancellationToken.None);
        _userRefs.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(existing);

        await _consumer.Consume(ctx);

        await _userRefs.Received(1).UpsertAsync(
            Arg.Is<UserRef>(u =>
                u.Id == userId
                && u.Email == "new@example.com"
                && u.FullName == "New Imported Seller"
                && u.UserName == "new-user"
                && u.PhoneNumber == "0123456789"
                && u.Locale == Culture.En
                && u.StoreName == "Existing Store"
                && u.StoreLogoUrl == "https://cdn.example.com/store.png"),
            Arg.Any<CancellationToken>());
    }

}
