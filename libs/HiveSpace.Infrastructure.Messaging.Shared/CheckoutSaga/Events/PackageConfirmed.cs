namespace HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Events;

public record PackageConfirmed
{
    public Guid           CorrelationId { get; init; }
    public Guid           OrderId       { get; init; }
    public Guid           PackageId     { get; init; }
    public Guid           StoreId       { get; init; }
    public DateTimeOffset ConfirmedAt   { get; init; }
}
