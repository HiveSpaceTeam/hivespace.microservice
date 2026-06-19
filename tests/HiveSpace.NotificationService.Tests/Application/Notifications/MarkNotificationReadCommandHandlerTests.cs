using FluentAssertions;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.NotificationService.Core.DomainModels;
using HiveSpace.NotificationService.Core.Features.Notifications.Commands.MarkNotificationRead;
using HiveSpace.NotificationService.Core.Persistence.Repositories;
using HiveSpace.NotificationService.Tests.Fixtures;
using HiveSpace.Testing.Shared.Doubles;
using Xunit;

namespace HiveSpace.NotificationService.Tests.Application.Notifications;

public class MarkNotificationReadCommandHandlerTests : IClassFixture<NotificationServiceFixture>
{
    private readonly NotificationServiceFixture _fixture;

    public MarkNotificationReadCommandHandlerTests(NotificationServiceFixture fixture)
        => _fixture = fixture;

    private MarkNotificationReadCommandHandler CreateHandler(Guid userId)
        => new(new NotificationRepository(_fixture.DbContext),
               new FakeUserContext { UserId = userId });

    [Fact]
    public async Task Handle_MarksNotificationAsRead()
    {
        var userId = Guid.NewGuid();
        var notification = Notification.Create(userId, NotificationChannel.InApp, "order.placed", $"idem-{Guid.NewGuid()}", "{}");
        notification.MarkSent();
        _fixture.DbContext.Notifications.Add(notification);
        await _fixture.DbContext.SaveChangesAsync();

        await CreateHandler(userId).Handle(new MarkNotificationReadCommand(notification.Id), default);

        var stored = await _fixture.DbContext.Notifications.FindAsync(notification.Id);
        stored!.Status.Should().Be(NotificationStatus.Read);
        stored.ReadAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_NotificationNotFound_ThrowsNotFoundException()
    {
        var act = () => CreateHandler(Guid.NewGuid()).Handle(
            new MarkNotificationReadCommand(Guid.NewGuid()), default);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_OtherUsersNotification_ThrowsForbiddenException()
    {
        var ownerId = Guid.NewGuid();
        var notification = Notification.Create(ownerId, NotificationChannel.InApp, "order.placed", $"idem-{Guid.NewGuid()}", "{}");
        _fixture.DbContext.Notifications.Add(notification);
        await _fixture.DbContext.SaveChangesAsync();

        var act = () => CreateHandler(Guid.NewGuid()).Handle(
            new MarkNotificationReadCommand(notification.Id), default);

        await act.Should().ThrowAsync<ForbiddenException>();
    }
}
