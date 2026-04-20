using HiveSpace.Domain.Shared.Enumerations;
using MassTransit;

namespace HiveSpace.OrderService.Infrastructure.Sagas;

public class FulfillmentSagaState : SagaStateMachineInstance
{
    public Guid   CorrelationId { get; set; }   // = OrderId
    public string CurrentState  { get; set; } = null!;

    // Carried from OrderReadyForFulfillment
    public Guid          UserId         { get; set; }
    public Guid          StoreId        { get; set; }
    public string        OrderCode      { get; set; } = string.Empty;
    public List<Guid>    ReservationIds { get; set; } = new();
    public long          GrandTotal     { get; set; }
    public PaymentMethod PaymentMethod  { get; set; } = PaymentMethod.COD;

    // Outcome flag — set when we know whether seller confirmed or rejected
    public bool OrderWasConfirmed { get; set; }

    // Tracking
    public DateTimeOffset  CreatedAt     { get; set; }
    public DateTimeOffset? CompletedAt   { get; set; }
    public DateTimeOffset? FailedAt      { get; set; }
    public string?         FailureReason { get; set; }

    // Scheduled message tokens
    public Guid? SellerConfirmationTimeoutTokenId { get; set; }
    public Guid? SagaStepTimeoutTokenId           { get; set; }
}
