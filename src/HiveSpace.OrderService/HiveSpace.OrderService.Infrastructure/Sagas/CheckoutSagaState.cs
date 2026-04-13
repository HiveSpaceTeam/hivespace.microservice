using HiveSpace.Domain.Shared.Enumerations;
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

    // Set after CreateOrder — list of created order IDs (one per store)
    public List<Guid>             OrderIds      { get; set; } = new();
    public Dictionary<Guid, Guid> OrderStoreMap { get; set; } = new();   // OrderId → StoreId
    public long                   GrandTotal    { get; set; }

    // Set after ReserveInventory
    public List<Guid>                   ReservationIds       { get; set; } = new();
    public Dictionary<Guid, List<Guid>> OrderReservationMap  { get; set; } = new();

    // Online payment (set after PaymentInitiation step)
    public Guid?           PaymentId        { get; set; }
    public string?         PaymentUrl       { get; set; }
    public DateTimeOffset? PaymentExpiresAt { get; set; }

    // Tracking
    public DateTimeOffset  CreatedAt     { get; set; }
    public DateTimeOffset? CompletedAt   { get; set; }
    public DateTimeOffset? FailedAt      { get; set; }
    public string?         FailureReason { get; set; }

    // Internal Request() pending token IDs
    public Guid? OrderCreationPendingTokenId        { get; set; }
    public Guid? InventoryReservationPendingTokenId { get; set; }
    public Guid? CODMarkingPendingTokenId           { get; set; }
    public Guid? CartClearingPendingTokenId         { get; set; }
    public Guid? PaymentInitiationPendingTokenId    { get; set; }
    public Guid? PaymentMarkingPendingTokenId       { get; set; }

    // Schedule token for payment timeout
    public Guid? PaymentTimeoutTokenId { get; set; }
}
