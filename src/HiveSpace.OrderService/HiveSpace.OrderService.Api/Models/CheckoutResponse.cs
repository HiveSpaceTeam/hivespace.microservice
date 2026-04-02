namespace HiveSpace.OrderService.Api.Models;

public record CheckoutResponse
{
    public List<Guid>      OrderIds         { get; init; } = new();
    public string          Status           { get; init; } = null!;
    public long            GrandTotal       { get; init; }
    public string?         PaymentUrl       { get; init; }
    public DateTimeOffset? PaymentExpiresAt { get; init; }
}
