namespace HiveSpace.OrderService.Application.Orders.Dtos;

public record BuyerOrderItemDto
{
    public Guid   Id            { get; init; }
    public string ProductName   { get; init; } = null!;
    public string ProductImage  { get; init; } = null!;
    public string Variation     { get; init; } = null!;
    public int    Quantity      { get; init; }
    public long   OriginalPrice { get; init; }
    public long   UnitPrice     { get; init; }
    public long   LineTotal     { get; init; }
    public string Currency      { get; init; } = null!;
}
