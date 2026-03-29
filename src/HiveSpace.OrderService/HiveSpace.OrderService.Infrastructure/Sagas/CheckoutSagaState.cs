using HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga;
using HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Dtos;
using MassTransit;

namespace HiveSpace.OrderService.Infrastructure.Sagas;

public class CheckoutSagaState : SagaStateMachineInstance
{
    public Guid   CorrelationId { get; set; }
    public string CurrentState  { get; set; } = null!;

    // For responding back to the IRequestClient caller
    public Guid? RequestId       { get; set; }
    public Uri?  ResponseAddress { get; set; }

    // User input
    public Guid               UserId          { get; set; }
    public DeliveryAddressDto DeliveryAddress { get; set; } = null!;
    public List<string>       CouponCodes     { get; set; } = new();
    public PaymentMethod      PaymentMethod   { get; set; } = PaymentMethod.COD;

    // Calculated during ValidateCheckout
    public List<OrderItemDto> Items          { get; set; } = new();
    public long               Subtotal       { get; set; }
    public long               ShippingFee    { get; set; }
    public long               TaxAmount      { get; set; }
    public long               DiscountAmount { get; set; }
    public long               GrandTotal     { get; set; }

    // Set after CreateOrder — CorrelationId IS the OrderId
    public List<Guid> PackageIds { get; set; } = new();

    // Set after ReserveInventory — carried into CheckoutPaymentSettled handoff
    public List<Guid>                   ReservationIds        { get; set; } = new();
    public Dictionary<Guid, List<Guid>> PackageReservationMap { get; set; } = new();

    // Phase 2 — online payment (unused for COD)
    public string?         PaymentUrl       { get; set; }
    public DateTimeOffset? PaymentExpiresAt { get; set; }

    // Tracking
    public DateTimeOffset  CreatedAt     { get; set; }
    public DateTimeOffset? CompletedAt   { get; set; }
    public DateTimeOffset? FailedAt      { get; set; }
    public string?         FailureReason { get; set; }

    // Internal Request() pending token IDs (MassTransit uses these to cancel scheduled timeouts)
    public Guid? CartValidationPendingTokenId       { get; set; }
    public Guid? OrderCreationPendingTokenId        { get; set; }
    public Guid? InventoryReservationPendingTokenId { get; set; }
    public Guid? CODMarkingPendingTokenId           { get; set; }
}
