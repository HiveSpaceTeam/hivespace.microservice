namespace HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Events;

public record PackageRejected
{
    public Guid    CorrelationId { get; init; }
    public Guid    OrderId       { get; init; }
    public Guid    PackageId     { get; init; }
    public string  Reason        { get; init; } = null!;
    public long    PackageAmount { get; init; }
}
