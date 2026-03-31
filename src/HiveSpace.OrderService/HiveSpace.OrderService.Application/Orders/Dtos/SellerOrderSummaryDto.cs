namespace HiveSpace.OrderService.Application.Orders.Dtos;

public record SellerOrderSummaryDto
{
    public Guid    Id          { get; init; }
    public Guid    StoreId     { get; init; }
    public string  Status      { get; init; } = null!;
    public long    SubTotal    { get; init; }
    public long    TotalAmount { get; init; }
    public string  Currency    { get; init; } = null!;
    public int     ItemCount   { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}
