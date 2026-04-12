namespace HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Commands;

public record MarkOrderAsPaid
{
    public Guid       CorrelationId { get; init; }
    public List<Guid> OrderIds      { get; init; } = [];
    public Guid       PaymentId     { get; init; }
}
