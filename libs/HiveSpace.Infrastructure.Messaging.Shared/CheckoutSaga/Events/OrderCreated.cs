namespace HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Events;

public record OrderCreated
{
    public Guid           CorrelationId { get; init; }
    public Guid           OrderId       { get; init; }
    public List<Guid>     PackageIds    { get; init; } = new();
    public DateTimeOffset CreatedAt     { get; init; }
}
