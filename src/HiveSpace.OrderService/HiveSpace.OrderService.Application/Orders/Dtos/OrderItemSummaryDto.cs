using HiveSpace.Application.Shared.Dtos;

namespace HiveSpace.OrderService.Application.Orders.Dtos;

public record OrderItemSummaryDto
{
    public Guid   Id          { get; init; }
    public long   ProductId   { get; init; }
    public long   SkuId       { get; init; }
    public string ProductName { get; init; } = null!;
    public string SkuName     { get; init; } = string.Empty;
    public string ImageUrl    { get; init; } = null!;
    public int    Quantity    { get; init; }
    public MoneyResponseDto UnitPrice   { get; init; } = null!;
    public MoneyResponseDto LineTotal   { get; init; } = null!;
    public bool   IsCOD       { get; init; }
    public MoneyResponseDto SnapshotPrice { get; init; } = null!;
    public DateTimeOffset SnapshotCapturedAt { get; init; }
    public Dictionary<string, string> Attributes { get; init; } = [];
}
