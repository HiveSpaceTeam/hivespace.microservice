namespace HiveSpace.OrderService.Application.Orders.Dtos;

public record OrderCheckoutDto
{
    public string PaymentMethod { get; init; } = null!;
    public long Amount { get; init; }
    public string Currency { get; init; } = null!;
    public DateTimeOffset CreatedAt { get; init; }
}
