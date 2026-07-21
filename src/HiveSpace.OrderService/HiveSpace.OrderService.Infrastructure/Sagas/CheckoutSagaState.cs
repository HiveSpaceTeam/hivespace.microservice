using HiveSpace.Domain.Shared.Enumerations;
using HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga;
using HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Commands;
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
    public CheckoutCouponSelectionDto CouponSelections { get; set; } = new();
    public PaymentMethod      PaymentMethod   { get; set; } = PaymentMethod.COD;

    // Set after CreateOrder — list of created order IDs (one per store)
    public List<Guid>             OrderIds      { get; set; } = new();
    public Dictionary<Guid, Guid>   OrderStoreMap { get; set; } = new();   // OrderId → StoreId
    public Dictionary<Guid, string> OrderCodeMap  { get; set; } = new();   // OrderId → OrderCode
    public Dictionary<Guid, long>   OrderAmountMap { get; set; } = new();
    public string                   CurrencyCode   { get; set; } = Currency.VND.GetCode();
    public List<CheckoutPaymentOrderDto> LinkedPaymentOrders { get; set; } = new();
    public long                     GrandTotal    { get; set; }

    // Set after ReserveInventory
    public List<Guid>                   ReservationIds       { get; set; } = new();
    public Dictionary<Guid, List<Guid>> OrderReservationMap  { get; set; } = new();

    // Online payment (set after PaymentInitiation step)
    public Guid?           PaymentId        { get; set; }
    public string?         PaymentReferenceNo { get; set; }
    public Guid?           CurrentPaymentAttemptId { get; set; }
    public int?            CurrentPaymentAttemptNo { get; set; }
    public DateTimeOffset? PaymentOutcomeAppliedAt { get; set; }
    public string?         PaymentUrl       { get; set; }
    public DateTimeOffset? PaymentExpiresAt { get; set; }

    // Compensation tracking
    public int PendingInventoryReleases { get; set; }

    // Tracking
    public DateTimeOffset  CreatedAt     { get; set; }
    public DateTimeOffset? CompletedAt   { get; set; }
    public DateTimeOffset? FailedAt      { get; set; }
    public string?         FailureReason { get; set; }

    // Internal Request() pending token IDs
    public Guid? OrderCreationPendingTokenId        { get; set; }
    public Guid? InventoryReservationPendingTokenId { get; set; }
    public Guid? CODMarkingPendingTokenId           { get; set; }
    public Guid? CouponUsageCommitPendingTokenId    { get; set; }
    public Guid? CartClearingPendingTokenId         { get; set; }
    public Guid? PaymentInitiationPendingTokenId    { get; set; }
    public Guid? PaymentMarkingPendingTokenId       { get; set; }

    // Schedule token for payment timeout
    public Guid? PaymentTimeoutTokenId { get; set; }

    public void RefreshLinkedPaymentOrders()
    {
        LinkedPaymentOrders = OrderIds
            .Select(orderId => new CheckoutPaymentOrderDto(
                orderId,
                OrderCodeMap.GetValueOrDefault(orderId, orderId.ToString()),
                OrderStoreMap.GetValueOrDefault(orderId),
                OrderAmountMap.GetValueOrDefault(orderId),
                CurrencyCode))
            .ToList();
    }

    public void RecordPaymentInitiated(
        Guid paymentId,
        string referenceNo,
        Guid attemptId,
        int attemptNo,
        string? paymentUrl,
        DateTimeOffset? expiresAt)
    {
        PaymentId = paymentId;
        PaymentReferenceNo = referenceNo;
        CurrentPaymentAttemptId = attemptId;
        CurrentPaymentAttemptNo = attemptNo;
        PaymentUrl = paymentUrl;
        PaymentExpiresAt = expiresAt;
    }

    public bool IsCurrentPaymentOutcome(
        Guid paymentId,
        Guid attemptId,
        int attemptNo,
        IReadOnlyList<CheckoutPaymentOrderDto> orders)
    {
        if (PaymentOutcomeAppliedAt.HasValue)
            return false;
        if (PaymentId != paymentId)
            return false;
        if (CurrentPaymentAttemptId != attemptId || CurrentPaymentAttemptNo != attemptNo)
            return false;

        var expected = LinkedPaymentOrders.Select(o => o.OrderId).OrderBy(x => x).ToArray();
        var actual = orders.Select(o => o.OrderId).OrderBy(x => x).ToArray();
        return expected.SequenceEqual(actual);
    }
}
