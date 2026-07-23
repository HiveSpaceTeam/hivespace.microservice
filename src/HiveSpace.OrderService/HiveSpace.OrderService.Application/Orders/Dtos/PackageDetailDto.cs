using HiveSpace.Application.Shared.Dtos;

namespace HiveSpace.OrderService.Application.Orders.Dtos;

public record PackageDetailDto
{
    public Guid    Id          { get; init; }
    public Guid    StoreId     { get; init; }
    public string  Status      { get; init; } = null!;
    public MoneyResponseDto SubTotal    { get; init; } = null!;
    public MoneyResponseDto TotalAmount { get; init; } = null!;
    public int     ItemCount   { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}
