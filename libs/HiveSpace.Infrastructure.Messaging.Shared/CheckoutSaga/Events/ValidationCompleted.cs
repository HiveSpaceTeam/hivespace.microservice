using HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Dtos;

namespace HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Events;

public record ValidationCompleted
{
    public Guid               CorrelationId  { get; init; }
    public List<OrderItemDto> Items          { get; init; } = new();
    public long               Subtotal       { get; init; }
    public long               ShippingFee    { get; init; }
    public long               TaxAmount      { get; init; }
    public long               DiscountAmount { get; init; }
    public long               GrandTotal     { get; init; }
}
