namespace HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Commands;

public record MarkOrderAsPaid
{
    public Guid       CorrelationId { get; init; }
    public List<Guid> OrderIds      { get; init; } = [];
    public Guid       PaymentId     { get; init; }
    public string?    PaymentReferenceNo { get; init; }
    public string?    MethodCode { get; init; }
    public Guid?      PaymentAttemptId { get; init; }
    public int?       AttemptNo { get; init; }
}
