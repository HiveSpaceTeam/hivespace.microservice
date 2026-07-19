namespace HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Commands;

public sealed record InitiatePayment
{
    public Guid CorrelationId { get; init; }
    public Guid BuyerId { get; init; }
    public string MethodCode { get; init; } = string.Empty;
    public long Amount { get; init; }
    public string CurrencyCode { get; init; } = string.Empty;
    public string IdempotencyKey { get; init; } = null!;
    public IReadOnlyList<CheckoutPaymentOrderDto> Orders { get; init; } = [];

    // Compatibility for the current OrderService saga until its 0011 slice is implemented.
    public List<Guid> OrderIds { get; init; } = [];
    public string Currency { get; init; } = "VND";
    public string Gateway { get; init; } = null!;
    public string ReturnUrl { get; init; } = null!;
    public string CancelUrl { get; init; } = null!;
}

public sealed record CheckoutPaymentOrderDto(
    Guid OrderId,
    string OrderCode,
    Guid StoreId,
    long Amount,
    string CurrencyCode);
