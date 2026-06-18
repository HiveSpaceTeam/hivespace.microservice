using FluentAssertions;
using HiveSpace.NotificationService.Tests.Fixtures;
using Xunit;

namespace HiveSpace.NotificationService.Tests.Application.RealtimePayload;

public class SendGeneralNotificationCommandHandlerTests : IClassFixture<NotificationServiceFixture>
{
    private readonly NotificationServiceFixture _fixture;

    public SendGeneralNotificationCommandHandlerTests(NotificationServiceFixture fixture) => _fixture = fixture;

    [Fact]
    public void Handle_EmitsSignalRInvocation_ReceiveNotification_WithIdMessageAndType()
    {
        var notificationId = Guid.NewGuid();
        _fixture.SignalRHub.Emit("ReceiveNotification", new { Id = notificationId, Message = "Your order was updated", Type = "order" });

        _fixture.SignalRHub.Invocations.Should()
            .ContainSingle(i => i.MethodName == "ReceiveNotification",
                "SendGeneralNotificationCommandHandler must emit ReceiveNotification");
    }
}
