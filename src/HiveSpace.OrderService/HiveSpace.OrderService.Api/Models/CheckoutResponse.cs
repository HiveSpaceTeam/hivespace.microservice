using HiveSpace.Application.Shared.Dtos;

namespace HiveSpace.OrderService.Api.Models;

public record CheckoutResponse
{
    public List<Guid>      OrderIds         { get; init; } = new();
    public string          Status           { get; init; } = null!;
    public MoneyResponseDto GrandTotal       { get; init; } = null!;
    public string?         PaymentUrl       { get; init; }
    public DateTimeOffset? PaymentExpiresAt { get; init; }
}
