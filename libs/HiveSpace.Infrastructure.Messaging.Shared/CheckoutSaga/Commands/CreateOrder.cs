using HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Dtos;

namespace HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Commands;

public record CreateOrder
{
    public Guid               CorrelationId   { get; init; }
    public Guid               UserId          { get; init; }
    public List<OrderItemDto> Items           { get; init; } = new();
    public DeliveryAddressDto DeliveryAddress { get; init; } = null!;
    public decimal            Subtotal        { get; init; }
    public decimal            ShippingFee     { get; init; }
    public decimal            TaxAmount       { get; init; }
    public decimal            DiscountAmount  { get; init; }
    public decimal            GrandTotal      { get; init; }
    public PaymentMethod      PaymentMethod   { get; init; } = PaymentMethod.COD;
}
