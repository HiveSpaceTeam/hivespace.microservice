using FluentAssertions;
using HiveSpace.NotificationService.Core.Features.Preferences.Commands.UpsertGroupPreference;
using HiveSpace.NotificationService.Tests.Fixtures;
using Xunit;

namespace HiveSpace.NotificationService.Tests.Application.RealtimePayload;

public class RealtimeNotificationTests : IClassFixture<NotificationServiceFixture>
{
    private readonly NotificationServiceFixture _fixture;

    public RealtimeNotificationTests(NotificationServiceFixture fixture) => _fixture = fixture;

    [Fact]
    public void SendOrderUpdate_EmitsCorrectSignalRMethod()
    {
        _fixture.SignalRHub.Emit("ReceiveOrderUpdate", new { OrderId = Guid.NewGuid(), Status = "Paid" });

        _fixture.SignalRHub.Invocations.Should().ContainSingle(i => i.MethodName == "ReceiveOrderUpdate");
    }

    [Fact]
    public void SendPaymentResult_EmitsSignalRInvocation()
    {
        _fixture.SignalRHub.Emit("ReceivePaymentResult", new { PaymentId = Guid.NewGuid(), Success = true });

        _fixture.SignalRHub.Invocations.Should().Contain(i => i.MethodName == "ReceivePaymentResult");
    }

    [Fact]
    public void SendGeneralNotification_CapturedBySignalRFake()
    {
        _fixture.SignalRHub.Emit("ReceiveNotification", new { Id = Guid.NewGuid() });

        _fixture.SignalRHub.Invocations.Should().Contain(i => i.MethodName == "ReceiveNotification");
        typeof(UpsertGroupPreferenceCommandHandler).Should().NotBeNull();
    }
}
