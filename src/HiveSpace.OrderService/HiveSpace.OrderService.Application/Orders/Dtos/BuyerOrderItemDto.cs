using HiveSpace.Application.Shared.Dtos;

namespace HiveSpace.OrderService.Application.Orders.Dtos;

public record BuyerOrderItemDto
{
    public Guid   Id            { get; init; }
    public string ProductName   { get; init; } = null!;
    public string ProductImage  { get; init; } = null!;
    public string Variation     { get; init; } = null!;
    public int    Quantity      { get; init; }
    public MoneyResponseDto OriginalPrice { get; init; } = null!;
    public MoneyResponseDto UnitPrice     { get; init; } = null!;
    public MoneyResponseDto LineTotal     { get; init; } = null!;
}
