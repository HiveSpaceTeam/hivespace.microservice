namespace HiveSpace.OrderService.Application.Orders.Dtos;

public record SellerOrderSummaryDto
{
    public Guid    Id            { get; init; }
    public string  OrderCode     { get; init; } = null!;
    public string  BuyerName     { get; init; } = null!;
    public string  Status        { get; init; } = null!;
    public string? PaymentMethod { get; init; }
    public long    TotalAmount   { get; init; }
    public DateTimeOffset ActionDateTime { get; init; }
    public DateTimeOffset CreatedAt     { get; init; }
    public List<SellerOrderItemDto> Items { get; init; } = [];
}
