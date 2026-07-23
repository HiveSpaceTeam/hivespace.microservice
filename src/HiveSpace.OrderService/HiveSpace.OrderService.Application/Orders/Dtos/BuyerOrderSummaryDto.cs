using HiveSpace.Application.Shared.Dtos;

namespace HiveSpace.OrderService.Application.Orders.Dtos;

public record BuyerOrderSummaryDto
{
    public Guid   Id          { get; init; }
    public string OrderCode   { get; init; } = null!;
    public string Status      { get; init; } = null!;
    public Guid? PaymentId { get; init; }
    public string? PaymentReferenceNo { get; init; }
    public string? PaymentMethodCode { get; init; }
    public int? PaymentAttemptNo { get; init; }
    public MoneyResponseDto TotalAmount { get; init; } = null!;
    public DateTimeOffset CreatedAt  { get; init; }
    public int    ItemCount   { get; init; }
    public List<BuyerOrderItemDto> Items { get; init; } = [];
}
