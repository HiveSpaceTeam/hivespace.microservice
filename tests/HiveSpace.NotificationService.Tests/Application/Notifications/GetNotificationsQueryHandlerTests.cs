using FluentAssertions;
using HiveSpace.NotificationService.Core.DomainModels;
using HiveSpace.NotificationService.Core.Features.Notifications.Queries.GetNotifications;
using HiveSpace.NotificationService.Core.Persistence.Repositories;
using HiveSpace.NotificationService.Tests.Fixtures;
using HiveSpace.Testing.Shared.Doubles;
using Xunit;

namespace HiveSpace.NotificationService.Tests.Application.Notifications;

public class GetNotificationsQueryHandlerTests : IClassFixture<NotificationServiceFixture>
{
    private readonly NotificationServiceFixture _fixture;

    public GetNotificationsQueryHandlerTests(NotificationServiceFixture fixture)
        => _fixture = fixture;

    private GetNotificationsQueryHandler CreateHandler(Guid userId)
        => new(new NotificationRepository(_fixture.DbContext),
               new FakeUserContext { UserId = userId });

    [Fact]
    public async Task Handle_ReturnsSentAndReadInAppNotificationsForUser()
    {
        var userId = Guid.NewGuid();
        var sent = Notification.Create(userId, NotificationChannel.InApp, "order.placed", $"idem-{Guid.NewGuid()}", "{}");
        sent.MarkSent();
        var read = Notification.Create(userId, NotificationChannel.InApp, "order.confirmed", $"idem-{Guid.NewGuid()}", "{}");
        read.MarkSent();
        read.MarkRead();
        _fixture.DbContext.Notifications.AddRange(sent, read);
        await _fixture.DbContext.SaveChangesAsync();

        var result = await CreateHandler(userId).Handle(new GetNotificationsQuery(1, 10, false), default);

        result.Notifications.Should().HaveCount(2);
        result.HasMore.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WithUnreadOnly_ReturnsSentNotificationsOnly()
    {
        var userId = Guid.NewGuid();
        var sent = Notification.Create(userId, NotificationChannel.InApp, "order.shipped", $"idem-{Guid.NewGuid()}", "{}");
        sent.MarkSent();
        var alreadyRead = Notification.Create(userId, NotificationChannel.InApp, "order.delivered", $"idem-{Guid.NewGuid()}", "{}");
        alreadyRead.MarkSent();
        alreadyRead.MarkRead();
        _fixture.DbContext.Notifications.AddRange(sent, alreadyRead);
        await _fixture.DbContext.SaveChangesAsync();

        var result = await CreateHandler(userId).Handle(new GetNotificationsQuery(1, 10, true), default);

        result.Notifications.Should().HaveCount(1);
        result.Notifications[0].Status.Should().Be(NotificationStatus.Sent);
    }

    [Fact]
    public async Task Handle_HasMore_WhenResultsExceedPageSize()
    {
        var userId = Guid.NewGuid();
        for (var i = 0; i < 3; i++)
        {
            var n = Notification.Create(userId, NotificationChannel.InApp, "order.placed", $"idem-{Guid.NewGuid()}", "{}");
            n.MarkSent();
            _fixture.DbContext.Notifications.Add(n);
        }
        await _fixture.DbContext.SaveChangesAsync();

        var result = await CreateHandler(userId).Handle(new GetNotificationsQuery(1, 2, false), default);

        result.Notifications.Should().HaveCount(2);
        result.HasMore.Should().BeTrue();
    }
}
