using MassTransit;

namespace HiveSpace.OrderService.Infrastructure.Sagas;

public class FulfillmentSagaState : SagaStateMachineInstance
{
    public Guid   CorrelationId { get; set; }   // = OrderId
    public string CurrentState  { get; set; } = null!;

    // Carried from CheckoutPaymentSettled
    public Guid               UserId                { get; set; }
    public List<Guid>         PackageIds            { get; set; } = new();
    public List<Guid>         ReservationIds        { get; set; } = new();
    public Dictionary<Guid, List<Guid>> PackageReservationMap { get; set; } = new();
    public long               GrandTotal            { get; set; }

    // Package confirmation tracking
    public int        TotalPackages       { get; set; }
    public int        ConfirmedPackages   { get; set; }
    public int        RejectedPackages    { get; set; }
    public List<Guid> ConfirmedPackageIds { get; set; } = new();
    public List<Guid> RejectedPackageIds  { get; set; } = new();

    // Tracking
    public DateTimeOffset  CreatedAt     { get; set; }
    public DateTimeOffset? CompletedAt   { get; set; }
    public DateTimeOffset? FailedAt      { get; set; }
    public string?         FailureReason { get; set; }

    // Scheduled message tokens
    public Guid? SellerConfirmationTimeoutTokenId { get; set; }   // 3-day timeout
    public Guid? SagaStepTimeoutTokenId           { get; set; }   // 30-min step timeout
}
