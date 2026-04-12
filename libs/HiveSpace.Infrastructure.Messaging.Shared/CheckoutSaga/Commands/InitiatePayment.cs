namespace HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Commands;

public record InitiatePayment
{
    public Guid CorrelationId { get; init; }
    public List<Guid> OrderIds { get; init; } = [];
    public Guid BuyerId { get; init; }
    public long Amount { get; init; }
    public string Currency { get; init; } = "VND";
    public string Gateway { get; init; } = null!;
    public string ReturnUrl { get; init; } = null!;
    public string CancelUrl { get; init; } = null!;
    public string IdempotencyKey { get; init; } = null!;
}
