namespace HiveSpace.OrderService.Api.Models;

public record CheckoutResponse
{
    public Guid            OrderId          { get; init; }
    public string          Status           { get; init; } = null!;
    public long            GrandTotal       { get; init; }
    public string?         PaymentUrl       { get; init; }
    public DateTimeOffset? PaymentExpiresAt { get; init; }
}
