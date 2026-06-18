using FluentAssertions;
using HiveSpace.NotificationService.Core.DomainModels;
using HiveSpace.NotificationService.Core.Features.Notifications.Queries.GetUnreadCount;
using HiveSpace.NotificationService.Core.Persistence.Repositories;
using HiveSpace.NotificationService.Tests.Fixtures;
using HiveSpace.Testing.Shared.Doubles;
using Xunit;

namespace HiveSpace.NotificationService.Tests.Application.Notifications;

public class GetUnreadCountQueryHandlerTests : IClassFixture<NotificationServiceFixture>
{
    private readonly NotificationServiceFixture _fixture;

    public GetUnreadCountQueryHandlerTests(NotificationServiceFixture fixture)
        => _fixture = fixture;

    private GetUnreadCountQueryHandler CreateHandler(Guid userId)
        => new(new NotificationRepository(_fixture.DbContext),
               new FakeUserContext { UserId = userId });

    [Fact]
    public async Task Handle_CountsSentInAppNotifications()
    {
        var userId = Guid.NewGuid();
        var n1 = Notification.Create(userId, NotificationChannel.InApp, "order.placed", $"idem-{Guid.NewGuid()}", "{}");
        n1.MarkSent();
        var n2 = Notification.Create(userId, NotificationChannel.InApp, "order.confirmed", $"idem-{Guid.NewGuid()}", "{}");
        n2.MarkSent();
        _fixture.DbContext.Notifications.AddRange(n1, n2);
        await _fixture.DbContext.SaveChangesAsync();

        var count = await CreateHandler(userId).Handle(new GetUnreadCountQuery(), default);

        count.Should().Be(2);
    }

    [Fact]
    public async Task Handle_ExcludesReadNotifications()
    {
        var userId = Guid.NewGuid();
        var sent = Notification.Create(userId, NotificationChannel.InApp, "order.placed", $"idem-{Guid.NewGuid()}", "{}");
        sent.MarkSent();
        var alreadyRead = Notification.Create(userId, NotificationChannel.InApp, "order.confirmed", $"idem-{Guid.NewGuid()}", "{}");
        alreadyRead.MarkSent();
        alreadyRead.MarkRead();
        _fixture.DbContext.Notifications.AddRange(sent, alreadyRead);
        await _fixture.DbContext.SaveChangesAsync();

        var count = await CreateHandler(userId).Handle(new GetUnreadCountQuery(), default);

        count.Should().Be(1);
    }

    [Fact]
    public async Task Handle_ReturnsZero_WhenNoUnreadNotifications()
    {
        var userId = Guid.NewGuid();
        var count = await CreateHandler(userId).Handle(new GetUnreadCountQuery(), default);
        count.Should().Be(0);
    }
}
