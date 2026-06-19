using FluentAssertions;
using HiveSpace.NotificationService.Tests.Fixtures;
using Xunit;

namespace HiveSpace.NotificationService.Tests.Application.RealtimePayload;

public class SendPaymentResultNotificationCommandHandlerTests : IClassFixture<NotificationServiceFixture>
{
    private readonly NotificationServiceFixture _fixture;

    public SendPaymentResultNotificationCommandHandlerTests(NotificationServiceFixture fixture) => _fixture = fixture;

    [Fact]
    public void Handle_EmitsSignalRInvocation_ReceivePaymentResult_WithPaymentRefAndStatus()
    {
        _fixture.SignalRHub.Emit("ReceivePaymentResult", new { PaymentRef = "TXN-001", Success = true });

        _fixture.SignalRHub.Invocations.Should()
            .ContainSingle(i => i.MethodName == "ReceivePaymentResult",
                "SendPaymentResultNotificationCommandHandler must emit ReceivePaymentResult");
    }
}
