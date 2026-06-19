using System.Reflection;
using FluentAssertions;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.Infrastructure.Messaging.Shared.Events.Media;
using HiveSpace.UserService.Api.Consumers;
using HiveSpace.UserService.Application.Interfaces.Messaging;
using HiveSpace.UserService.Domain.Aggregates.Store;
using HiveSpace.UserService.Domain.Aggregates.User;
using HiveSpace.UserService.Infrastructure.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace HiveSpace.UserService.Tests.Consumers;

public class MediaAssetProcessedConsumerTests
{
    private UserDbContext CreateDb() =>
        new(new DbContextOptionsBuilder<UserDbContext>()
            .UseInMemoryDatabase($"user-media-{Guid.NewGuid()}")
            .Options);

    [Fact]
    public async Task Consume_UnknownEntityType_DoesNotThrow()
    {
        var db = CreateDb();
        var consumer = new MediaAssetProcessedConsumer(db, Substitute.For<IStoreEventPublisher>(), Substitute.For<ILogger<MediaAssetProcessedConsumer>>());
        var msg = new MediaAssetProcessedIntegrationEvent("file1", "other_type", null, "https://cdn.example.com/img.jpg", null);
        var ctx = Substitute.For<ConsumeContext<MediaAssetProcessedIntegrationEvent>>();
        ctx.Message.Returns(msg);
        ctx.CancellationToken.Returns(CancellationToken.None);

        var act = async () => await consumer.Consume(ctx);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Consume_UserAvatar_WhenUserNotFound_ThrowsNotFoundException()
    {
        var db = CreateDb();
        var consumer = new MediaAssetProcessedConsumer(db, Substitute.For<IStoreEventPublisher>(), Substitute.For<ILogger<MediaAssetProcessedConsumer>>());
        var msg = new MediaAssetProcessedIntegrationEvent("avatar-file", "user_avatar", null, "https://cdn.example.com/avatar.jpg", null);
        var ctx = Substitute.For<ConsumeContext<MediaAssetProcessedIntegrationEvent>>();
        ctx.Message.Returns(msg);
        ctx.CancellationToken.Returns(CancellationToken.None);

        var act = async () => await consumer.Consume(ctx);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Consume_StoreLogo_WhenStoreNotFound_ThrowsNotFoundException()
    {
        var db = CreateDb();
        var consumer = new MediaAssetProcessedConsumer(db, Substitute.For<IStoreEventPublisher>(), Substitute.For<ILogger<MediaAssetProcessedConsumer>>());
        var msg = new MediaAssetProcessedIntegrationEvent("logo-file", "store_logo", null, "https://cdn.example.com/logo.jpg", null);
        var ctx = Substitute.For<ConsumeContext<MediaAssetProcessedIntegrationEvent>>();
        ctx.Message.Returns(msg);
        ctx.CancellationToken.Returns(CancellationToken.None);

        var act = async () => await consumer.Consume(ctx);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Consume_UserAvatar_WhenUserFound_UpdatesAvatarUrl()
    {
        var db = CreateDb();
        var fileId = "avatar-file-id";
        var user = User.CreateProfile(Guid.NewGuid(), Email.Create("user@example.com"), "testuser", "Test User");
        user.SetAvatar(fileId);
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var consumer = new MediaAssetProcessedConsumer(db, Substitute.For<IStoreEventPublisher>(), Substitute.For<ILogger<MediaAssetProcessedConsumer>>());
        var msg = new MediaAssetProcessedIntegrationEvent(fileId, "user_avatar", null, "https://cdn.example.com/avatar.jpg", null);
        var ctx = Substitute.For<ConsumeContext<MediaAssetProcessedIntegrationEvent>>();
        ctx.Message.Returns(msg);
        ctx.CancellationToken.Returns(CancellationToken.None);

        await consumer.Consume(ctx);

        db.Users.First(u => u.AvatarFileId == fileId).AvatarUrl.Should().Be("https://cdn.example.com/avatar.jpg");
    }

    [Fact]
    public async Task Consume_StoreLogo_WhenStoreFound_UpdatesLogoUrlAndPublishes()
    {
        var db = CreateDb();
        var fileId = "logo-file-id";
        var createMethod = typeof(Store).GetMethod("Create",
            BindingFlags.NonPublic | BindingFlags.Static, null,
            new[] { typeof(string), typeof(string), typeof(string), typeof(string), typeof(Guid), typeof(Guid?) },
            null)!;
        var store = (Store)createMethod.Invoke(null, new object?[] { "Test Store", null, fileId, "123 Main St", Guid.NewGuid(), (Guid?)null })!;
        db.Stores.Add(store);
        await db.SaveChangesAsync();

        var publisher = Substitute.For<IStoreEventPublisher>();
        var consumer = new MediaAssetProcessedConsumer(db, publisher, Substitute.For<ILogger<MediaAssetProcessedConsumer>>());
        var msg = new MediaAssetProcessedIntegrationEvent(fileId, "store_logo", null, "https://cdn.example.com/logo.jpg", null);
        var ctx = Substitute.For<ConsumeContext<MediaAssetProcessedIntegrationEvent>>();
        ctx.Message.Returns(msg);
        ctx.CancellationToken.Returns(CancellationToken.None);

        await consumer.Consume(ctx);

        await publisher.Received(1).PublishStoreUpdatedAsync(Arg.Any<Store>(), Arg.Any<CancellationToken>());
    }
}
