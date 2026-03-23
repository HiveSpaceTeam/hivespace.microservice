using HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga;
using HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Dtos;
using MassTransit;

namespace HiveSpace.OrderService.Infrastructure.Sagas;

public class CheckoutSagaState : SagaStateMachineInstance
{
    public Guid   CorrelationId { get; set; }
    public string CurrentState  { get; set; } = null!;

    public Guid               UserId          { get; set; }
    public DeliveryAddressDto DeliveryAddress { get; set; } = null!;
    public List<string>       CouponCodes     { get; set; } = new();

    public Guid       OrderId           { get; set; }
    public List<Guid> PackageIds        { get; set; } = new();
    public int        TotalPackages     { get; set; }
    public int        ConfirmedPackages { get; set; }
    public int        RejectedPackages  { get; set; }
    public List<Guid> RejectedPackageIds { get; set; } = new();

    public List<OrderItemDto> Items          { get; set; } = new();
    public decimal            Subtotal       { get; set; }
    public decimal            ShippingFee    { get; set; }
    public decimal            TaxAmount      { get; set; }
    public decimal            DiscountAmount { get; set; }
    public decimal            GrandTotal     { get; set; }

    public List<Guid>                   ReservationIds        { get; set; } = new();
    public Dictionary<Guid, List<Guid>> PackageReservationMap { get; set; } = new();

    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.COD;

    public string?         FailureReason { get; set; }
    public DateTimeOffset? FailedAt      { get; set; }

    public DateTimeOffset  CreatedAt   { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }

    public Guid? SellerConfirmationTimeoutTokenId { get; set; }
    public Guid? SagaStepTimeoutTokenId           { get; set; }
}
