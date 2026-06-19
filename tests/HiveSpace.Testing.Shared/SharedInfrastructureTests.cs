using FluentAssertions;
using HiveSpace.Infrastructure.Messaging.Events;
using HiveSpace.Testing.Shared.Builders;
using HiveSpace.Testing.Shared.Capture;
using HiveSpace.Testing.Shared.Doubles;
using HiveSpace.Testing.Shared.Fakes;
using Xunit;

namespace HiveSpace.Testing.Shared;

public class SharedInfrastructureTests
{
    [Fact]
    public async Task Fakes_ShouldCaptureDeterministicSideEffects()
    {
        var now = new DateTimeOffset(2026, 6, 12, 0, 0, 0, TimeSpan.Zero);
        var clock = new DeterministicClock(now);
        var user = FakeCurrentUser.Create("seller", storeId: TestDataBuilder.NewUlid().ToString());
        var messages = new InMemoryMessageCapture();
        var email = new EmailDeliveryFake();
        var blob = new BlobStorageFake();
        var hub = new SignalRHubFake();
        var payment = new PaymentProviderFake();

        payment.SetupReturn("txn-1", new VNPayResult(true, "txn-1"));
        await messages.PublishAsync(new IntegrationEvent());
        await email.SendAsync("buyer@hivespace.local", "Subject", "Body");
        await blob.ConfirmUploadAsync("asset-1");
        hub.Emit("ReceiveNotification", "payload");

        clock.GetUtcNow().Should().Be(now);
        user.Identity?.IsAuthenticated.Should().BeTrue();
        messages.Published.Should().ContainSingle();
        email.Sent.Should().ContainSingle();
        blob.ConfirmedKeys.Should().Contain("asset-1");
        hub.Invocations.Should().ContainSingle(i => i.MethodName == "ReceiveNotification");
        payment.GetStub("txn-1").Success.Should().BeTrue();
    }
}
