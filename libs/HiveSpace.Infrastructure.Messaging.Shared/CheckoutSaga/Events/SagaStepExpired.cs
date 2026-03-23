namespace HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Events;

public record SagaStepExpired
{
    public Guid CorrelationId { get; init; }
    public Guid OrderId       { get; init; }
}
