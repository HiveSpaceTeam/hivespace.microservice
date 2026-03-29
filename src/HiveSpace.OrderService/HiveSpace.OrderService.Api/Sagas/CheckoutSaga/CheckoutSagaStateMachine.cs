using HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Commands;
using HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Events;
using HiveSpace.OrderService.Infrastructure.Sagas;
using MassTransit;

namespace HiveSpace.OrderService.Api.Sagas.CheckoutSaga;

public class CheckoutSagaStateMachine : MassTransitStateMachine<CheckoutSagaState>
{
    public CheckoutSagaStateMachine()
    {
        InstanceState(x => x.CurrentState);

        Event(() => CheckoutInitiated,           x => x.CorrelateById(m => m.Message.CorrelationId));
        Event(() => ValidationCompleted,         x => x.CorrelateById(m => m.Message.CorrelationId));
        Event(() => ValidationFailed,            x => x.CorrelateById(m => m.Message.CorrelationId));
        Event(() => OrderCreated,                x => x.CorrelateById(m => m.Message.CorrelationId));
        Event(() => InventoryReserved,           x => x.CorrelateById(m => m.Message.CorrelationId));
        Event(() => InventoryReservationFailed,  x => x.CorrelateById(m => m.Message.CorrelationId));
        Event(() => OrderMarkedAsCOD,            x => x.CorrelateById(m => m.Message.CorrelationId));
        Event(() => InventoryConfirmed,          x => x.CorrelateById(m => m.Message.CorrelationId));
        Event(() => InventoryConfirmationFailed, x => x.CorrelateById(m => m.Message.CorrelationId));
        Event(() => SellersNotified,             x => x.CorrelateById(m => m.Message.CorrelationId));
        Event(() => PackageConfirmed,            x => x.CorrelateById(m => m.Message.CorrelationId));
        Event(() => PackageRejected,             x => x.CorrelateById(m => m.Message.CorrelationId));
        Event(() => CustomerNotified,            x => x.CorrelateById(m => m.Message.CorrelationId));
        Event(() => InventoryReleased,           x => x.CorrelateById(m => m.Message.CorrelationId));
        Event(() => OrderCancelled,              x => x.CorrelateById(m => m.Message.CorrelationId));
        Event(() => MarkOrderAsCODFailed,        x => x.CorrelateById(m => m.Message.CorrelationId));

        Schedule(() => SellerConfirmationTimeout, x => x.SellerConfirmationTimeoutTokenId, s =>
        {
            s.Delay = TimeSpan.FromDays(3);
        });

        Schedule(() => SagaStepTimeout, x => x.SagaStepTimeoutTokenId, s =>
        {
            s.Delay = TimeSpan.FromMinutes(30);
        });

        // ── INITIAL ──────────────────────────────────────────────────────────
        Initially(
            When(CheckoutInitiated)
                .Then(ctx =>
                {
                    ctx.Saga.UserId          = ctx.Message.UserId;
                    ctx.Saga.DeliveryAddress = ctx.Message.DeliveryAddress;
                    ctx.Saga.CouponCodes     = ctx.Message.CouponCodes;
                    ctx.Saga.PaymentMethod   = ctx.Message.PaymentMethod;
                    ctx.Saga.CreatedAt       = DateTimeOffset.UtcNow;
                })
                .TransitionTo(Validating)
                .Schedule(SagaStepTimeout, ctx => ctx.Init<SagaStepExpired>(new
                {
                    CorrelationId = ctx.Saga.CorrelationId,
                    OrderId       = Guid.Empty
                }))
                .PublishAsync(ctx => ctx.Init<ValidateCheckout>(new
                {
                    ctx.Message.CorrelationId,
                    ctx.Message.UserId,
                    ctx.Message.CouponCodes,
                    ctx.Message.DeliveryAddress
                }))
        );

        // ── VALIDATING ───────────────────────────────────────────────────────
        During(Validating,
            When(SagaStepTimeout.Received)
                .Then(ctx =>
                {
                    ctx.Saga.FailureReason = $"Saga timed out waiting for validation (state: {ctx.Saga.CurrentState})";
                    ctx.Saga.FailedAt      = DateTimeOffset.UtcNow;
                })
                .TransitionTo(Failed)
                .Finalize(),

            When(ValidationCompleted)
                .Then(ctx =>
                {
                    ctx.Saga.Items          = ctx.Message.Items;
                    ctx.Saga.Subtotal       = ctx.Message.Subtotal;
                    ctx.Saga.ShippingFee    = ctx.Message.ShippingFee;
                    ctx.Saga.TaxAmount      = ctx.Message.TaxAmount;
                    ctx.Saga.DiscountAmount = ctx.Message.DiscountAmount;
                    ctx.Saga.GrandTotal     = ctx.Message.GrandTotal;
                })
                .TransitionTo(CreatingOrder)
                .PublishAsync(ctx => ctx.Init<CreateOrder>(new
                {
                    ctx.Message.CorrelationId,
                    UserId          = ctx.Saga.UserId,
                    Items           = ctx.Saga.Items,
                    DeliveryAddress = ctx.Saga.DeliveryAddress,
                    Subtotal        = ctx.Saga.Subtotal,
                    ShippingFee     = ctx.Saga.ShippingFee,
                    TaxAmount       = ctx.Saga.TaxAmount,
                    DiscountAmount  = ctx.Saga.DiscountAmount,
                    GrandTotal      = ctx.Saga.GrandTotal,
                    PaymentMethod   = ctx.Saga.PaymentMethod
                })),

            When(ValidationFailed)
                .Then(ctx =>
                {
                    ctx.Saga.FailureReason = ctx.Message.Reason;
                    ctx.Saga.FailedAt      = DateTimeOffset.UtcNow;
                })
                .TransitionTo(Failed)
                .Finalize()
        );

        // ── CREATING ORDER ────────────────────────────────────────────────────
        During(CreatingOrder,
            When(SagaStepTimeout.Received)
                .Then(ctx =>
                {
                    ctx.Saga.FailureReason = $"Saga timed out waiting for order creation (state: {ctx.Saga.CurrentState})";
                    ctx.Saga.FailedAt      = DateTimeOffset.UtcNow;
                })
                .TransitionTo(Compensating)
                .PublishAsync(ctx => ctx.Init<CancelOrder>(new
                {
                    ctx.Saga.CorrelationId,
                    OrderId = ctx.Saga.OrderId,
                    Reason  = ctx.Saga.FailureReason
                })),

            When(OrderCreated)
                .Then(ctx =>
                {
                    ctx.Saga.OrderId       = ctx.Message.OrderId;
                    ctx.Saga.PackageIds    = ctx.Message.PackageIds;
                    ctx.Saga.TotalPackages = ctx.Message.PackageIds.Count;
                })
                .TransitionTo(ReservingInventory)
                .PublishAsync(ctx => ctx.Init<ReserveInventory>(new
                {
                    ctx.Message.CorrelationId,
                    OrderId           = ctx.Saga.OrderId,
                    Items             = ctx.Saga.Items,
                    ExpirationMinutes = 15
                }))
        );

        // ── RESERVING INVENTORY ───────────────────────────────────────────────
        During(ReservingInventory,
            When(SagaStepTimeout.Received)
                .Then(ctx =>
                {
                    ctx.Saga.FailureReason = $"Saga timed out waiting for inventory reservation (state: {ctx.Saga.CurrentState})";
                    ctx.Saga.FailedAt      = DateTimeOffset.UtcNow;
                })
                .TransitionTo(Compensating)
                .PublishAsync(ctx => ctx.Init<ReleaseInventory>(new
                {
                    ctx.Saga.CorrelationId,
                    OrderId        = ctx.Saga.OrderId,
                    ReservationIds = ctx.Saga.ReservationIds
                })),

            When(InventoryReserved)
                .Then(ctx =>
                {
                    ctx.Saga.ReservationIds        = ctx.Message.ReservationIds;
                    ctx.Saga.PackageReservationMap = ctx.Message.PackageReservationMap;
                })
                .TransitionTo(MarkingAsCOD)
                .PublishAsync(ctx => ctx.Init<MarkOrderAsCOD>(new
                {
                    ctx.Message.CorrelationId,
                    OrderId = ctx.Saga.OrderId
                })),

            When(InventoryReservationFailed)
                .Then(ctx =>
                {
                    ctx.Saga.FailureReason = $"Inventory reservation failed: {ctx.Message.Reason}";
                    ctx.Saga.FailedAt      = DateTimeOffset.UtcNow;
                })
                .TransitionTo(Compensating)
                .PublishAsync(ctx => ctx.Init<CancelOrder>(new
                {
                    ctx.Message.CorrelationId,
                    OrderId = ctx.Saga.OrderId,
                    Reason  = ctx.Saga.FailureReason
                }))
        );

        // ── MARKING AS COD ────────────────────────────────────────────────────
        During(MarkingAsCOD,
            When(SagaStepTimeout.Received)
                .Then(ctx =>
                {
                    ctx.Saga.FailureReason = $"Saga timed out waiting for COD marking (state: {ctx.Saga.CurrentState})";
                    ctx.Saga.FailedAt      = DateTimeOffset.UtcNow;
                })
                .TransitionTo(Compensating)
                .PublishAsync(ctx => ctx.Init<ReleaseInventory>(new
                {
                    ctx.Saga.CorrelationId,
                    OrderId        = ctx.Saga.OrderId,
                    ReservationIds = ctx.Saga.ReservationIds
                })),

            When(OrderMarkedAsCOD)
                .TransitionTo(ConfirmingInventory)
                .PublishAsync(ctx => ctx.Init<ConfirmInventory>(new
                {
                    ctx.Message.CorrelationId,
                    OrderId        = ctx.Saga.OrderId,
                    ReservationIds = ctx.Saga.ReservationIds
                })),

            When(MarkOrderAsCODFailed)
                .Then(ctx =>
                {
                    ctx.Saga.FailureReason = ctx.Message.Reason;
                    ctx.Saga.FailedAt      = DateTimeOffset.UtcNow;
                })
                .TransitionTo(Compensating)
                .PublishAsync(ctx => ctx.Init<ReleaseInventory>(new
                {
                    ctx.Message.CorrelationId,
                    OrderId        = ctx.Saga.OrderId,
                    ReservationIds = ctx.Saga.ReservationIds
                }))
        );

        // ── CONFIRMING INVENTORY ──────────────────────────────────────────────
        During(ConfirmingInventory,
            When(InventoryConfirmationFailed)
                .Then(ctx =>
                {
                    ctx.Saga.FailureReason = ctx.Message.Reason;
                    ctx.Saga.FailedAt      = DateTimeOffset.UtcNow;
                })
                .TransitionTo(Compensating)
                .PublishAsync(ctx => ctx.Init<ReleaseInventory>(new
                {
                    ctx.Message.CorrelationId,
                    OrderId        = ctx.Saga.OrderId,
                    ReservationIds = ctx.Saga.ReservationIds
                })),

            When(SagaStepTimeout.Received)
                .Then(ctx =>
                {
                    ctx.Saga.FailureReason = $"Saga timed out waiting for inventory confirmation (state: {ctx.Saga.CurrentState})";
                    ctx.Saga.FailedAt      = DateTimeOffset.UtcNow;
                })
                .TransitionTo(Compensating)
                .PublishAsync(ctx => ctx.Init<ReleaseInventory>(new
                {
                    ctx.Saga.CorrelationId,
                    OrderId        = ctx.Saga.OrderId,
                    ReservationIds = ctx.Saga.ReservationIds
                })),

            When(InventoryConfirmed)
                .TransitionTo(NotifyingSellers)
                .PublishAsync(ctx => ctx.Init<NotifySellers>(new
                {
                    ctx.Message.CorrelationId,
                    OrderId    = ctx.Saga.OrderId,
                    PackageIds = ctx.Saga.PackageIds
                }))
        );

        // ── NOTIFYING SELLERS ─────────────────────────────────────────────────
        During(NotifyingSellers,
            When(SagaStepTimeout.Received)
                .Then(ctx =>
                {
                    ctx.Saga.FailureReason = $"Saga timed out waiting for seller notification (state: {ctx.Saga.CurrentState})";
                    ctx.Saga.FailedAt      = DateTimeOffset.UtcNow;
                })
                .TransitionTo(Compensating)
                .PublishAsync(ctx => ctx.Init<ReleaseInventory>(new
                {
                    ctx.Saga.CorrelationId,
                    OrderId        = ctx.Saga.OrderId,
                    ReservationIds = ctx.Saga.ReservationIds
                })),

            When(SellersNotified)
                .Unschedule(SagaStepTimeout)
                .Schedule(SellerConfirmationTimeout, ctx => ctx.Init<SellerConfirmationExpired>(new
                {
                    CorrelationId = ctx.Saga.CorrelationId,
                    OrderId       = ctx.Saga.OrderId
                }))
                .TransitionTo(WaitingForPackageConfirmation)
        );

        // ── WAITING FOR PACKAGE CONFIRMATION ─────────────────────────────────
        During(WaitingForPackageConfirmation,
            When(PackageConfirmed)
                .Then(ctx => ctx.Saga.ConfirmedPackages++)
                .If(ctx => ctx.Saga.ConfirmedPackages + ctx.Saga.RejectedPackages >= ctx.Saga.TotalPackages &&
                            ctx.Saga.ConfirmedPackages > 0,
                    binder => binder
                        .Unschedule(SellerConfirmationTimeout)
                        .If(ctx => ctx.Saga.RejectedPackages > 0,
                            inner => inner.PublishAsync(ctx =>
                            {
                                var reservationsToRelease = ctx.Saga.RejectedPackageIds
                                    .SelectMany(pkgId => ctx.Saga.PackageReservationMap.TryGetValue(pkgId, out var ids)
                                        ? ids
                                        : Enumerable.Empty<Guid>())
                                    .ToList();
                                return ctx.Init<ReleaseInventory>(new
                                {
                                    ctx.Message.CorrelationId,
                                    OrderId        = ctx.Saga.OrderId,
                                    ReservationIds = reservationsToRelease
                                });
                            }))
                        .TransitionTo(NotifyingCustomer)
                        .PublishAsync(ctx => ctx.Init<NotifyCustomer>(new
                        {
                            ctx.Message.CorrelationId,
                            OrderId              = ctx.Saga.OrderId,
                            UserId               = ctx.Saga.UserId,
                            IsPartialOrder       = ctx.Saga.RejectedPackages > 0,
                            RejectedPackageCount = ctx.Saga.RejectedPackages
                        }))),

            When(PackageRejected)
                .Then(ctx =>
                {
                    ctx.Saga.RejectedPackages++;
                    ctx.Saga.RejectedPackageIds.Add(ctx.Message.PackageId);
                })
                .If(ctx => ctx.Saga.RejectedPackages >= ctx.Saga.TotalPackages,
                    binder => binder
                        .Then(ctx =>
                        {
                            ctx.Saga.FailureReason = "All packages were rejected by sellers";
                            ctx.Saga.FailedAt      = DateTimeOffset.UtcNow;
                        })
                        .Unschedule(SellerConfirmationTimeout)
                        .TransitionTo(Compensating)
                        .PublishAsync(ctx => ctx.Init<ReleaseInventory>(new
                        {
                            ctx.Message.CorrelationId,
                            OrderId        = ctx.Saga.OrderId,
                            ReservationIds = ctx.Saga.ReservationIds
                        })))
                .If(ctx =>
                    ctx.Saga.ConfirmedPackages + ctx.Saga.RejectedPackages >= ctx.Saga.TotalPackages &&
                    ctx.Saga.ConfirmedPackages > 0,
                    binder => binder
                        .Unschedule(SellerConfirmationTimeout)
                        .PublishAsync(ctx =>
                        {
                            var reservationsToRelease = ctx.Saga.RejectedPackageIds
                                .SelectMany(pkgId => ctx.Saga.PackageReservationMap.TryGetValue(pkgId, out var ids)
                                    ? ids
                                    : Enumerable.Empty<Guid>())
                                .ToList();
                            return ctx.Init<ReleaseInventory>(new
                            {
                                ctx.Message.CorrelationId,
                                OrderId        = ctx.Saga.OrderId,
                                ReservationIds = reservationsToRelease
                            });
                        })
                        .TransitionTo(NotifyingCustomer)
                        .PublishAsync(ctx => ctx.Init<NotifyCustomer>(new
                        {
                            ctx.Message.CorrelationId,
                            OrderId              = ctx.Saga.OrderId,
                            UserId               = ctx.Saga.UserId,
                            IsPartialOrder       = true,
                            RejectedPackageCount = ctx.Saga.RejectedPackages
                        }))),

            When(SellerConfirmationTimeout.Received)
                .Then(ctx =>
                {
                    ctx.Saga.FailureReason = "Seller confirmation timed out after 3 days";
                    ctx.Saga.FailedAt      = DateTimeOffset.UtcNow;
                })
                .TransitionTo(Compensating)
                .PublishAsync(ctx => ctx.Init<ReleaseInventory>(new
                {
                    ctx.Saga.CorrelationId,
                    OrderId        = ctx.Saga.OrderId,
                    ReservationIds = ctx.Saga.ReservationIds
                }))
        );

        // ── NOTIFYING CUSTOMER ────────────────────────────────────────────────
        During(NotifyingCustomer,
            When(CustomerNotified)
                .Then(ctx => ctx.Saga.CompletedAt = DateTimeOffset.UtcNow)
                .TransitionTo(Completed)
                .Finalize()
        );

        // ── COMPENSATING ──────────────────────────────────────────────────────
        During(Compensating,
            When(InventoryReleased)
                .PublishAsync(ctx => ctx.Init<CancelOrder>(new
                {
                    ctx.Message.CorrelationId,
                    OrderId = ctx.Saga.OrderId,
                    Reason  = ctx.Saga.FailureReason
                })),

            When(OrderCancelled)
                .TransitionTo(Failed)
                .Finalize()
        );

        SetCompletedWhenFinalized();
    }

    public State Validating                    { get; private set; } = null!;
    public State CreatingOrder                 { get; private set; } = null!;
    public State ReservingInventory            { get; private set; } = null!;
    public State MarkingAsCOD                  { get; private set; } = null!;
    public State ConfirmingInventory           { get; private set; } = null!;
    public State NotifyingSellers              { get; private set; } = null!;
    public State WaitingForPackageConfirmation { get; private set; } = null!;
    public State NotifyingCustomer             { get; private set; } = null!;
    public State Completed                     { get; private set; } = null!;
    public State Failed                        { get; private set; } = null!;
    public State Compensating                  { get; private set; } = null!;

    public Event<CheckoutInitiated>           CheckoutInitiated          { get; private set; } = null!;
    public Event<ValidationCompleted>         ValidationCompleted        { get; private set; } = null!;
    public Event<ValidationFailed>            ValidationFailed           { get; private set; } = null!;
    public Event<OrderCreated>                OrderCreated               { get; private set; } = null!;
    public Event<InventoryReserved>           InventoryReserved          { get; private set; } = null!;
    public Event<InventoryReservationFailed>  InventoryReservationFailed { get; private set; } = null!;
    public Event<OrderMarkedAsCOD>            OrderMarkedAsCOD           { get; private set; } = null!;
    public Event<InventoryConfirmed>          InventoryConfirmed         { get; private set; } = null!;
    public Event<InventoryConfirmationFailed> InventoryConfirmationFailed { get; private set; } = null!;
    public Event<SellersNotified>             SellersNotified            { get; private set; } = null!;
    public Event<PackageConfirmed>            PackageConfirmed           { get; private set; } = null!;
    public Event<PackageRejected>             PackageRejected            { get; private set; } = null!;
    public Event<CustomerNotified>            CustomerNotified           { get; private set; } = null!;
    public Event<InventoryReleased>           InventoryReleased          { get; private set; } = null!;
    public Event<OrderCancelled>              OrderCancelled             { get; private set; } = null!;
    public Event<MarkOrderAsCODFailed>        MarkOrderAsCODFailed       { get; private set; } = null!;

    public Schedule<CheckoutSagaState, SellerConfirmationExpired> SellerConfirmationTimeout { get; private set; } = null!;
    public Schedule<CheckoutSagaState, SagaStepExpired>           SagaStepTimeout           { get; private set; } = null!;
}
