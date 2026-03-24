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

        // TODO: implement release logic (idempotent — mark reservations as Released)

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
