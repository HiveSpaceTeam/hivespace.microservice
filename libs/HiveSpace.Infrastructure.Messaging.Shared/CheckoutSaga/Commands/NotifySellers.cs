namespace HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Commands;

public record NotifySellers
{
    public Guid       CorrelationId { get; init; }
    public Guid       OrderId       { get; init; }
    public List<Guid> PackageIds    { get; init; } = new();
}
