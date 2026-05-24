using HiveSpace.Infrastructure.Messaging.Events;
using HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Dtos;
namespace HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Events;

public record InventoryReservationFailedIntegrationEvent : IntegrationEvent
{
    public Guid                      CorrelationId { get; init; }
    public List<Guid> OrderIds { get; init; } = new();
    public string                    Reason        { get; init; } = null!;
    public List<StockFailureDto>     Failures      { get; init; } = new();
}
