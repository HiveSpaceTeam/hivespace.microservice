namespace HiveSpace.OrderService.Application.Orders.Dtos;

public record OrderSummaryDto
{
    public Guid    Id           { get; init; }
    public string  ShortId      { get; init; } = null!;
    public string  Status       { get; init; } = null!;
    public decimal TotalAmount  { get; init; }
    public string  Currency     { get; init; } = null!;
    public DateTimeOffset CreatedAt    { get; init; }
    public int     PackageCount { get; init; }
}
