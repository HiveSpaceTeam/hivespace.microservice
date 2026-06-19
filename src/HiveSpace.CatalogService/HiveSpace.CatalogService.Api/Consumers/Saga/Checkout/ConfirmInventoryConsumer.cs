using HiveSpace.Infrastructure.Messaging.Shared.FulfillmentSaga.Commands;
using HiveSpace.Infrastructure.Messaging.Shared.FulfillmentSaga.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace HiveSpace.CatalogService.Api.Consumers.Saga.Checkout;

public class ConfirmInventoryConsumer : IConsumer<ConfirmInventory>
{
    private readonly ILogger<ConfirmInventoryConsumer> _logger;

    public ConfirmInventoryConsumer(ILogger<ConfirmInventoryConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ConfirmInventory> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "ConfirmInventory received for order {OrderId} — {Count} reservation(s)",
            message.OrderId, message.ReservationIds.Count);

        // TODO: implement confirmation logic (Soft → Hard transition, check for expired reservations)
        _logger.LogInformation("Inventory confirmed for order {OrderId}", message.OrderId);

        await context.RespondAsync<InventoryConfirmedIntegrationEvent>(new
        {
            message.CorrelationId,
            message.OrderId,
            message.ReservationIds
        });
    }
}
