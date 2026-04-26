namespace HiveSpace.OrderService.Application.Orders.Dtos;

public record BuyerOrderSummaryDto
{
    public Guid   Id          { get; init; }
    public string OrderCode   { get; init; } = null!;
    public string Status      { get; init; } = null!;
    public long   TotalAmount { get; init; }
    public string Currency    { get; init; } = null!;
    public DateTimeOffset CreatedAt  { get; init; }
    public int    ItemCount   { get; init; }
    public List<BuyerOrderItemDto> Items { get; init; } = [];
}
