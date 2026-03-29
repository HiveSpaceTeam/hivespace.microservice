using HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Commands;
using HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace HiveSpace.CatalogService.Api.Consumers.Saga.Checkout;

public class ReserveInventoryConsumer : IConsumer<ReserveInventory>
{
    private readonly ILogger<ReserveInventoryConsumer> _logger;

    public ReserveInventoryConsumer(ILogger<ReserveInventoryConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ReserveInventory> context)
    {
        var message = context.Message;
        var ct      = context.CancellationToken;

        _logger.LogInformation(
            "ReserveInventory received for order {OrderId} — {ItemCount} item(s), expiration {ExpirationMinutes}m",
            message.OrderId, message.Items.Count, message.ExpirationMinutes);

        // TODO: implement reservation logic
        var reservationIds        = new List<Guid>();
        var packageReservationMap = new Dictionary<Guid, List<Guid>>();
        var failures              = new List<StockFailureDto>();

        var success = failures.Count == 0;

        if (success)
        {
            _logger.LogInformation(
                "Inventory reserved for order {OrderId} — {Count} reservation(s)",
                message.OrderId, reservationIds.Count);

            await context.RespondAsync<InventoryReserved>(new
            {
                message.CorrelationId,
                message.OrderId,
                ReservationIds        = reservationIds,
                ExpiresAt             = DateTimeOffset.UtcNow.AddMinutes(message.ExpirationMinutes),
                PackageReservationMap = packageReservationMap
            });
        }
        else
        {
            _logger.LogWarning(
                "Inventory reservation failed for order {OrderId} — {FailureCount} failure(s)",
                message.OrderId, failures.Count);

            await context.RespondAsync<InventoryReservationFailed>(new
            {
                message.CorrelationId,
                message.OrderId,
                Reason   = "One or more items could not be reserved",
                Failures = failures
            });
        }
    }
}
