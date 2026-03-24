namespace HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Commands;

public record MarkOrderAsCOD
{
    public Guid CorrelationId { get; init; }
    public Guid OrderId       { get; init; }
}
