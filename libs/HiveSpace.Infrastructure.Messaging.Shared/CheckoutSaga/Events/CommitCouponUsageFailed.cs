namespace HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Events;

public record CommitCouponUsageFailed
{
    public Guid CorrelationId { get; init; }
    public string Reason { get; init; } = string.Empty;
    public List<string> Errors { get; init; } = [];
}
