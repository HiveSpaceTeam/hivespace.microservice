namespace HiveSpace.OrderService.Application.Orders.Dtos;

public record OrderItemSummaryDto
{
    public Guid   Id          { get; init; }
    public long   ProductId   { get; init; }
    public long   SkuId       { get; init; }
    public string ProductName { get; init; } = null!;
    public string ImageUrl    { get; init; } = null!;
    public int    Quantity    { get; init; }
    public long   UnitPrice   { get; init; }
    public long   LineTotal   { get; init; }
    public string Currency    { get; init; } = null!;
    public bool   IsCOD       { get; init; }
}
