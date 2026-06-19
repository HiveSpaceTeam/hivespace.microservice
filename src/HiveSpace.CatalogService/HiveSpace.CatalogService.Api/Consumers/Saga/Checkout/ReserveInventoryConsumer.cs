using HiveSpace.CatalogService.Domain.Exceptions;
using HiveSpace.Domain.Shared.Exceptions;
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

        if (message.OrderIds.Count == 0)
            throw new InvalidFieldException(CatalogDomainErrorCode.InvalidQuantity, nameof(message.OrderIds));

        _logger.LogInformation(
            "ReserveInventory received for order {OrderIds} — {ItemCount} item(s), expiration {ExpirationMinutes}m",
            String.Join(",", message.OrderIds), message.Items.Count, message.ExpirationMinutes);

        // TODO: implement reservation logic
        var reservationIds        = new List<Guid>();
        var packageReservationMap = new Dictionary<Guid, List<Guid>>();

        _logger.LogInformation(
            "Inventory reserved for order {OrderId} — {Count} reservation(s)",
            String.Join(",", message.OrderIds), reservationIds.Count);

        await context.RespondAsync<InventoryReservedIntegrationEvent>(new
        {
            message.CorrelationId,
            message.OrderIds,
            ReservationIds      = reservationIds,
            ExpiresAt           = DateTimeOffset.UtcNow.AddMinutes(message.ExpirationMinutes),
            OrderReservationMap = packageReservationMap
        });
    }
}
