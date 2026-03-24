namespace HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Events;

public record CustomerNotified
{
    public Guid           CorrelationId { get; init; }
    public Guid           OrderId       { get; init; }
    public DateTimeOffset NotifiedAt    { get; init; }
}
