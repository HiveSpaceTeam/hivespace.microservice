using HiveSpace.Application.Shared.Dtos;

namespace HiveSpace.OrderService.Application.Orders.Dtos;

public record OrderCheckoutDto
{
    public string PaymentMethod { get; init; } = null!;
    public MoneyResponseDto Amount { get; init; } = null!;
    public DateTimeOffset CreatedAt { get; init; }
}
