namespace HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Events;

public record OrderCreationFailed
{
    public Guid         CorrelationId { get; init; }
    public string       Reason        { get; init; } = null!;
    public List<string> Errors        { get; init; } = new();
}
