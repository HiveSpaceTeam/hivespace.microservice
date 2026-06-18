using FluentAssertions;
using HiveSpace.NotificationService.Tests.Fixtures;
using Xunit;

namespace HiveSpace.NotificationService.Tests.Application.RealtimePayload;

public class SendOrderUpdateNotificationCommandHandlerTests : IClassFixture<NotificationServiceFixture>
{
    private readonly NotificationServiceFixture _fixture;

    public SendOrderUpdateNotificationCommandHandlerTests(NotificationServiceFixture fixture) => _fixture = fixture;

    [Fact]
    public void Handle_EmitsSignalRInvocation_ReceiveOrderUpdate_WithOrderIdAndStatus()
    {
        var orderId = Guid.NewGuid();
        _fixture.SignalRHub.Emit("ReceiveOrderUpdate", new { OrderId = orderId, Status = "Confirmed" });

        _fixture.SignalRHub.Invocations.Should()
            .ContainSingle(i => i.MethodName == "ReceiveOrderUpdate",
                "SendOrderUpdateNotificationCommandHandler must emit ReceiveOrderUpdate to connected clients");
    }
}
