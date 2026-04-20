using HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Commands;
using HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Events;
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
        var ct      = context.CancellationToken;

        _logger.LogInformation(
            "ConfirmInventory received for order {OrderId} — {Count} reservation(s)",
            message.OrderId, message.ReservationIds.Count);

        // TODO: implement confirmation logic (Soft → Hard transition, check for expired reservations)
        var expiredIds = new List<Guid>();

        var success = expiredIds.Count == 0;

        if (success)
        {
            _logger.LogInformation(
                "Inventory confirmed for order {OrderId}",
                message.OrderId);

            await context.RespondAsync<InventoryConfirmed>(new
            {
                message.CorrelationId,
                message.OrderId,
                message.ReservationIds
            });
        }
        else
        {
            _logger.LogWarning(
                "Inventory confirmation failed for order {OrderId} — {Count} reservation(s) expired",
                message.OrderId, expiredIds.Count);

            await context.RespondAsync<InventoryConfirmationFailed>(new
            {
                message.CorrelationId,
                message.OrderId,
                Reason     = $"{expiredIds.Count} reservation(s) expired before confirmation",
                ExpiredIds = expiredIds
            });
        }
    }
}
