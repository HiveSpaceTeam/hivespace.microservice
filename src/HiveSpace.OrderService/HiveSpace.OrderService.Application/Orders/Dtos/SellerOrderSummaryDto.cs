using HiveSpace.Application.Shared.Dtos;

namespace HiveSpace.OrderService.Application.Orders.Dtos;

public record SellerOrderSummaryDto
{
    public Guid    Id            { get; init; }
    public string  OrderCode     { get; init; } = null!;
    public string  BuyerName     { get; init; } = null!;
    public string  Status        { get; init; } = null!;
    public string? PaymentMethod { get; init; }
    public Guid?   PaymentId { get; init; }
    public string? PaymentReferenceNo { get; init; }
    public string? PaymentMethodCode { get; init; }
    public int?    PaymentAttemptNo { get; init; }
    public MoneyResponseDto TotalAmount   { get; init; } = null!;
    public DateTimeOffset ActionDateTime { get; init; }
    public DateTimeOffset CreatedAt     { get; init; }
    public List<SellerOrderItemDto> Items { get; init; } = [];
}
