namespace HiveSpace.OrderService.Application.Orders.Dtos;

public record PackageSummaryDto
{
    public Guid    Id          { get; init; }
    public Guid    OrderId     { get; init; }
    public Guid    StoreId     { get; init; }
    public string  Status      { get; init; } = null!;
    public decimal SubTotal    { get; init; }
    public decimal TotalAmount { get; init; }
    public string  Currency    { get; init; } = null!;
    public int     ItemCount   { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}
