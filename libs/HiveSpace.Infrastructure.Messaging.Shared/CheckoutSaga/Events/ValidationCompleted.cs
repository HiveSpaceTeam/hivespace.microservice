using HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Dtos;

namespace HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Events;

public record ValidationCompleted
{
    public Guid               CorrelationId  { get; init; }
    public List<OrderItemDto> Items          { get; init; } = new();
    public decimal            Subtotal       { get; init; }
    public decimal            ShippingFee    { get; init; }
    public decimal            TaxAmount      { get; init; }
    public decimal            DiscountAmount { get; init; }
    public decimal            GrandTotal     { get; init; }
}
