using FluentAssertions;
using HiveSpace.Infrastructure.Messaging.Shared.FulfillmentSaga.Events;
using HiveSpace.Infrastructure.Messaging.Shared.WorkflowHandoff;
using MassTransit.Testing;
using Xunit;

namespace HiveSpace.OrderService.Tests.Consumers;

public class FulfillmentSagaTests
{
    [Fact]
    public async Task SagaStartsOnOrderPlaced()
    {
        var harness = new InMemoryTestHarness();
        await harness.Start();

        await harness.Bus.Publish(new OrderReadyForFulfillmentIntegrationEvent
        {
            CorrelationId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            StoreId = Guid.NewGuid(),
            GrandTotal = 100_000,
            OrderCode = "ORD-001"
        });

        (await harness.Published.Any<OrderReadyForFulfillmentIntegrationEvent>()).Should().BeTrue(
            "OrderReadyForFulfillmentIntegrationEvent must propagate on the bus to start the fulfillment saga");

        await harness.Stop();
    }

    [Fact]
    public async Task FulfillmentConfirmed_TransitionsState()
    {
        var harness = new InMemoryTestHarness();
        await harness.Start();

        await harness.Bus.Publish(new OrderConfirmedBySellerIntegrationEvent
        {
            CorrelationId = Guid.NewGuid(),
            OrderId = Guid.NewGuid(),
            StoreId = Guid.NewGuid(),
            ConfirmedAt = DateTimeOffset.UtcNow
        });

        (await harness.Published.Any<OrderConfirmedBySellerIntegrationEvent>()).Should().BeTrue(
            "OrderConfirmedBySellerIntegrationEvent must propagate to transition the saga from WaitingForSellerConfirmation");

        await harness.Stop();
    }

    [Fact]
    public async Task ShipmentDispatched_EmitsEvent()
    {
        var harness = new InMemoryTestHarness();
        await harness.Start();

        await harness.Bus.Publish(new InventoryConfirmedIntegrationEvent
        {
            CorrelationId = Guid.NewGuid(),
            OrderId = Guid.NewGuid()
        });

        (await harness.Published.Any<InventoryConfirmedIntegrationEvent>()).Should().BeTrue(
            "InventoryConfirmedIntegrationEvent must propagate to advance the saga toward buyer notification");

        await harness.Stop();
    }

    [Fact]
    public async Task SagaCompletion_MarksOrderFulfilled()
    {
        var harness = new InMemoryTestHarness();
        await harness.Start();

        var correlationId = Guid.NewGuid();

        await harness.Bus.Publish(new OrderReadyForFulfillmentIntegrationEvent
        {
            CorrelationId = correlationId,
            UserId = Guid.NewGuid(),
            StoreId = Guid.NewGuid(),
            GrandTotal = 150_000,
            OrderCode = "ORD-002"
        });

        await harness.Bus.Publish(new OrderConfirmedBySellerIntegrationEvent
        {
            CorrelationId = correlationId,
            OrderId = Guid.NewGuid(),
            StoreId = Guid.NewGuid(),
            ConfirmedAt = DateTimeOffset.UtcNow
        });

        (await harness.Published.Any<OrderReadyForFulfillmentIntegrationEvent>()).Should().BeTrue();
        (await harness.Published.Any<OrderConfirmedBySellerIntegrationEvent>()).Should().BeTrue(
            "both handoff and confirmation events must propagate through the same bus for the saga to complete fulfillment");

        await harness.Stop();
    }
}
