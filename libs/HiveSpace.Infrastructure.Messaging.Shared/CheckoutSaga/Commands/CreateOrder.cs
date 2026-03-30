using HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Dtos;

namespace HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Commands;

public record CreateOrder
{
    public Guid               CorrelationId   { get; init; }
    public Guid               UserId          { get; init; }
    public List<OrderItemDto> Items           { get; init; } = new();
    public DeliveryAddressDto DeliveryAddress { get; init; } = null!;
    public long               Subtotal        { get; init; }
    public long               ShippingFee     { get; init; }
    public long               TaxAmount       { get; init; }
    public long               DiscountAmount  { get; init; }
    public long               GrandTotal      { get; init; }
    public PaymentMethod      PaymentMethod   { get; init; } = PaymentMethod.COD;
}
