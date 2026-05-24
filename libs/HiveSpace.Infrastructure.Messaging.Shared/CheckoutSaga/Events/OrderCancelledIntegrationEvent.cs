using HiveSpace.Infrastructure.Messaging.Events;
namespace HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Events;

public record OrderCancelledIntegrationEvent : IntegrationEvent
{
    public Guid           CorrelationId { get; init; }
    public Guid           OrderId       { get; init; }
    public string         Reason        { get; init; } = null!;
    public DateTimeOffset CancelledAt   { get; init; }
}
