namespace HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Commands;

public record MarkOrderAsCOD
{
    public Guid       CorrelationId { get; init; }
    public List<Guid> OrderIds      { get; init; } = new();
}
