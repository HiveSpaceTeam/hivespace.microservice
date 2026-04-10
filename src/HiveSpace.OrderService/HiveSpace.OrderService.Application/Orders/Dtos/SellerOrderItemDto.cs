namespace HiveSpace.OrderService.Application.Orders.Dtos;

public record SellerOrderItemDto
{
    public Guid    Id           { get; init; }
    public string  ProductName  { get; init; } = null!;
    public string  ProductImageUrl { get; init; } = null!;
    public string  Variation    { get; init; } = null!;
    public int     Quantity     { get; init; }
    public string? Tag          { get; init; }
}
