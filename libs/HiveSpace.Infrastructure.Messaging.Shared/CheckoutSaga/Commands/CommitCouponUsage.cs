namespace HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Commands;

public record CommitCouponUsage
{
    public Guid CorrelationId { get; init; }
    public List<Guid> OrderIds { get; init; } = [];
}
