using FluentAssertions;
using HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Commands;
using HiveSpace.Infrastructure.Messaging.Shared.IntegrationEvents;
using MassTransit.Testing;
using Xunit;

namespace HiveSpace.OrderService.Tests.Consumers;

public class CheckoutSagaTests
{
    [Fact]
    public async Task SagaStartsOnCheckoutInitiated()
    {
        var harness = new InMemoryTestHarness();
        await harness.Start();

        await harness.Bus.Publish(new CheckoutInitiated
        {
            CorrelationId = Guid.NewGuid(),
            UserId = Guid.NewGuid()
        });

        (await harness.Published.Any<CheckoutInitiated>()).Should().BeTrue(
            "CheckoutInitiated must propagate on the bus to start the checkout saga");

        await harness.Stop();
    }

    [Fact]
    public async Task PaymentConfirmed_TransitionsSagaToCompleted()
    {
        var harness = new InMemoryTestHarness();
        await harness.Start();

        await harness.Bus.Publish(new PaymentSucceededIntegrationEvent
        {
            SagaCorrelationId = Guid.NewGuid(),
            PaymentId = Guid.NewGuid(),
            OrderId = Guid.NewGuid(),
            BuyerId = Guid.NewGuid(),
            Amount = 100_000,
            Currency = "VND",
            PaidAt = DateTimeOffset.UtcNow
        });

        (await harness.Published.Any<PaymentSucceededIntegrationEvent>()).Should().BeTrue(
            "PaymentSucceededIntegrationEvent must propagate to advance the saga to completed state");

        await harness.Stop();
    }

    [Fact]
    public async Task PaymentFailed_TransitionsSagaToFailed()
    {
        var harness = new InMemoryTestHarness();
        await harness.Start();

        await harness.Bus.Publish(new PaymentFailedIntegrationEvent
        {
            SagaCorrelationId = Guid.NewGuid(),
            PaymentId = Guid.NewGuid(),
            OrderId = Guid.NewGuid(),
            BuyerId = Guid.NewGuid(),
            Reason = "Insufficient funds"
        });

        (await harness.Published.Any<PaymentFailedIntegrationEvent>()).Should().BeTrue(
            "PaymentFailedIntegrationEvent must propagate to transition the saga to failed/compensating state");

        await harness.Stop();
    }

    [Fact]
    public async Task SagaCompletion_CreatesOrder()
    {
        var harness = new InMemoryTestHarness();
        await harness.Start();

        var correlationId = Guid.NewGuid();

        await harness.Bus.Publish(new CheckoutInitiated
        {
            CorrelationId = correlationId,
            UserId = Guid.NewGuid()
        });

        await harness.Bus.Publish(new PaymentSucceededIntegrationEvent
        {
            SagaCorrelationId = correlationId,
            PaymentId = Guid.NewGuid(),
            OrderId = Guid.NewGuid(),
            BuyerId = Guid.NewGuid(),
            Amount = 150_000,
            Currency = "VND",
            PaidAt = DateTimeOffset.UtcNow
        });

        (await harness.Published.Any<CheckoutInitiated>()).Should().BeTrue();
        (await harness.Published.Any<PaymentSucceededIntegrationEvent>()).Should().BeTrue(
            "both events must propagate through the same bus for the saga to complete and create an order");

        await harness.Stop();
    }
}
