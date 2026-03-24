using HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Commands;
using HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace HiveSpace.CatalogService.Api.Consumers.Saga.Checkout;

public class ReleaseInventoryConsumer : IConsumer<ReleaseInventory>
{
    private readonly ILogger<ReleaseInventoryConsumer> _logger;

    public ReleaseInventoryConsumer(ILogger<ReleaseInventoryConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ReleaseInventory> context)
    {
        var message = context.Message;
        var ct      = context.CancellationToken;

        _logger.LogInformation(
            "ReleaseInventory received for order {OrderId} — {Count} reservation(s) to release",
            message.OrderId, message.ReservationIds.Count);

        // TODO: implement release logic (idempotent — mark reservations as Released).
        // Until the real release logic is in place this consumer must NOT emit InventoryReleased,
        // as doing so would let the saga treat inventory as freed when it was not.
        // Replace the throw below with the actual DB update before shipping to production.
        throw new NotImplementedException(
            "ReleaseInventoryConsumer is not yet implemented. " +
            "Reservations must be marked Released in the database before publishing InventoryReleased.");

        // --- placeholder below kept for reference; remove together with the throw above ---
        _logger.LogInformation(
            "Inventory released for order {OrderId}",
            message.OrderId);

        await context.Publish<InventoryReleased>(new
        {
            message.CorrelationId,
            message.OrderId,
            message.ReservationIds
        }, ct);
    }
}
