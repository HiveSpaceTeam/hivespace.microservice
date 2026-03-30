using HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Commands;
using HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Events;
using HiveSpace.OrderService.Infrastructure.Sagas;
using MassTransit;

namespace HiveSpace.OrderService.Api.Sagas.FulfillmentSaga;

public class FulfillmentSagaStateMachine : MassTransitStateMachine<FulfillmentSagaState>
{
    // ── Request (sync — consumer must RespondAsync) ───────────────────────────
    public Request<FulfillmentSagaState, ConfirmInventory, InventoryConfirmed, InventoryConfirmationFailed>
        InventoryConfirmation { get; private set; } = null!;

    // ── Scheduled timeout (3-day seller confirmation window) ─────────────────
    public Schedule<FulfillmentSagaState, SellerConfirmationExpired> SellerConfirmationTimeout { get; private set; } = null!;

    // ── Events (pub/sub) ──────────────────────────────────────────────────────
    public Event<CheckoutPaymentSettled> CheckoutPaymentSettled { get; private set; } = null!;
    public Event<SellersNotified>        SellersNotified        { get; private set; } = null!;
    public Event<PackageConfirmed>       PackageConfirmed       { get; private set; } = null!;
    public Event<PackageRejected>        PackageRejected        { get; private set; } = null!;
    public Event<CustomerNotified>       CustomerNotified       { get; private set; } = null!;
    public Event<InventoryReleased>      InventoryReleased      { get; private set; } = null!;
    public Event<OrderCancelled>         OrderCancelled         { get; private set; } = null!;

    // ── States ────────────────────────────────────────────────────────────────
    public State ConfirmingInventory           { get; private set; } = null!;
    public State NotifyingSellers              { get; private set; } = null!;
    public State WaitingForPackageConfirmation { get; private set; } = null!;
    public State NotifyingCustomer             { get; private set; } = null!;
    public State Completed                     { get; private set; } = null!;
    public State Compensating                  { get; private set; } = null!;
    public State Failed                        { get; private set; } = null!;

    public FulfillmentSagaStateMachine()
    {
        InstanceState(x => x.CurrentState);

        Request(() => InventoryConfirmation, x => x.SagaStepTimeoutTokenId, cfg => cfg.Timeout = TimeSpan.FromMinutes(30));

        Schedule(() => SellerConfirmationTimeout, x => x.SellerConfirmationTimeoutTokenId, cfg =>
        {
            cfg.Delay    = TimeSpan.FromDays(3);
            cfg.Received = e => e.CorrelateById(m => m.Message.CorrelationId);
        });

        Event(() => CheckoutPaymentSettled, x => x.CorrelateById(m => m.Message.CorrelationId));
        Event(() => SellersNotified,        x => x.CorrelateById(m => m.Message.CorrelationId));
        Event(() => PackageConfirmed,       x => x.CorrelateById(m => m.Message.CorrelationId));
        Event(() => PackageRejected,        x => x.CorrelateById(m => m.Message.CorrelationId));
        Event(() => CustomerNotified,       x => x.CorrelateById(m => m.Message.CorrelationId));
        Event(() => InventoryReleased,      x => x.CorrelateById(m => m.Message.CorrelationId));
        Event(() => OrderCancelled,         x => x.CorrelateById(m => m.Message.CorrelationId));

        // ── INITIAL ──────────────────────────────────────────────────────────
        Initially(
            When(CheckoutPaymentSettled)
                .Then(ctx =>
                {
                    ctx.Saga.UserId                = ctx.Message.UserId;
                    ctx.Saga.PackageIds            = ctx.Message.PackageIds;
                    ctx.Saga.ReservationIds        = ctx.Message.ReservationIds;
                    ctx.Saga.PackageReservationMap = ctx.Message.PackageReservationMap;
                    ctx.Saga.GrandTotal            = ctx.Message.GrandTotal;
                    ctx.Saga.TotalPackages         = ctx.Message.PackageIds.Count;
                    ctx.Saga.CreatedAt             = DateTimeOffset.UtcNow;
                })
                .Request(InventoryConfirmation, ctx => ctx.Init<ConfirmInventory>(new
                {
                    ctx.Saga.CorrelationId,
                    OrderId        = ctx.Saga.CorrelationId,
                    ReservationIds = ctx.Saga.ReservationIds
                }))
        );

        // ── CONFIRMING INVENTORY ─────────────────────────────────────────────
        During(InventoryConfirmation.Pending,
            When(InventoryConfirmation.Completed)   // InventoryConfirmed
                .TransitionTo(NotifyingSellers)
                .PublishAsync(ctx => ctx.Init<NotifySellers>(new
                {
                    ctx.Saga.CorrelationId,
                    OrderId    = ctx.Saga.CorrelationId,
                    PackageIds = ctx.Saga.PackageIds
                })),

            When(InventoryConfirmation.Completed2)  // InventoryConfirmationFailed
                .Then(ctx =>
                {
                    ctx.Saga.FailureReason = ctx.Message.Reason;
                    ctx.Saga.FailedAt      = DateTimeOffset.UtcNow;
                })
                .TransitionTo(Compensating)
                .PublishAsync(ctx => ctx.Init<CancelOrder>(new
                {
                    ctx.Saga.CorrelationId,
                    OrderId = ctx.Saga.CorrelationId,
                    Reason  = ctx.Saga.FailureReason
                })),

            When(InventoryConfirmation.Faulted)
                .Then(ctx =>
                {
                    ctx.Saga.FailureReason = "Unexpected error during inventory confirmation";
                    ctx.Saga.FailedAt      = DateTimeOffset.UtcNow;
                })
                .TransitionTo(Compensating)
                .PublishAsync(ctx => ctx.Init<CancelOrder>(new
                {
                    ctx.Saga.CorrelationId,
                    OrderId = ctx.Saga.CorrelationId,
                    Reason  = ctx.Saga.FailureReason
                })),

            When(InventoryConfirmation.TimeoutExpired)
                .Then(ctx =>
                {
                    ctx.Saga.FailureReason = "Inventory confirmation timed out";
                    ctx.Saga.FailedAt      = DateTimeOffset.UtcNow;
                })
                .TransitionTo(Compensating)
                .PublishAsync(ctx => ctx.Init<CancelOrder>(new
                {
                    ctx.Saga.CorrelationId,
                    OrderId = ctx.Saga.CorrelationId,
                    Reason  = ctx.Saga.FailureReason
                }))
        );

        // ── NOTIFYING SELLERS ────────────────────────────────────────────────
        During(NotifyingSellers,
            When(SellersNotified)
                .Schedule(SellerConfirmationTimeout, ctx => ctx.Init<SellerConfirmationExpired>(new
                {
                    ctx.Saga.CorrelationId,
                    OrderId = ctx.Saga.CorrelationId
                }))
                .TransitionTo(WaitingForPackageConfirmation)
        );

        // ── WAITING FOR PACKAGE CONFIRMATION ─────────────────────────────────
        During(WaitingForPackageConfirmation,
            When(PackageConfirmed)
                .Then(ctx =>
                {
                    ctx.Saga.ConfirmedPackages++;
                    ctx.Saga.ConfirmedPackageIds.Add(ctx.Message.PackageId);
                })
                .If(ctx => AllPackagesResolved(ctx.Saga), resolved => resolved
                    .Unschedule(SellerConfirmationTimeout)
                    .If(ctx => ctx.Saga.RejectedPackages > 0, hasRejected => hasRejected
                        .PublishAsync(ctx => ctx.Init<ReleaseInventory>(new
                        {
                            ctx.Saga.CorrelationId,
                            OrderId        = ctx.Saga.CorrelationId,
                            ReservationIds = GetRejectedReservationIds(ctx.Saga)
                        }))
                    )
                    .TransitionTo(NotifyingCustomer)
                    .PublishAsync(ctx => ctx.Init<NotifyCustomer>(new
                    {
                        ctx.Saga.CorrelationId,
                        OrderId              = ctx.Saga.CorrelationId,
                        UserId               = ctx.Saga.UserId,
                        IsPartialOrder       = ctx.Saga.RejectedPackages > 0,
                        RejectedPackageCount = ctx.Saga.RejectedPackages,
                        RefundAmount         = 0L
                    }))
                ),

            When(PackageRejected)
                .Then(ctx =>
                {
                    ctx.Saga.RejectedPackages++;
                    if (!ctx.Saga.RejectedPackageIds.Contains(ctx.Message.PackageId))
                        ctx.Saga.RejectedPackageIds.Add(ctx.Message.PackageId);
                })
                .If(ctx => AllPackagesResolved(ctx.Saga), resolved => resolved
                    .Unschedule(SellerConfirmationTimeout)
                    .IfElse(ctx => ctx.Saga.ConfirmedPackages == 0,
                        allRejected => allRejected
                            .Then(ctx =>
                            {
                                ctx.Saga.FailureReason = "All packages rejected by sellers";
                                ctx.Saga.FailedAt      = DateTimeOffset.UtcNow;
                            })
                            .TransitionTo(Compensating)
                            .PublishAsync(ctx => ctx.Init<ReleaseInventory>(new
                            {
                                ctx.Saga.CorrelationId,
                                OrderId        = ctx.Saga.CorrelationId,
                                ReservationIds = ctx.Saga.ReservationIds
                            })),
                        partial => partial
                            .PublishAsync(ctx => ctx.Init<ReleaseInventory>(new
                            {
                                ctx.Saga.CorrelationId,
                                OrderId        = ctx.Saga.CorrelationId,
                                ReservationIds = GetRejectedReservationIds(ctx.Saga)
                            }))
                            .TransitionTo(NotifyingCustomer)
                            .PublishAsync(ctx => ctx.Init<NotifyCustomer>(new
                            {
                                ctx.Saga.CorrelationId,
                                OrderId              = ctx.Saga.CorrelationId,
                                UserId               = ctx.Saga.UserId,
                                IsPartialOrder       = true,
                                RejectedPackageCount = ctx.Saga.RejectedPackages,
                                RefundAmount         = 0L
                            }))
                    )
                ),

            When(SellerConfirmationTimeout.Received)
                .Then(ctx =>
                {
                    var pending = ctx.Saga.PackageIds
                        .Except(ctx.Saga.ConfirmedPackageIds)
                        .Except(ctx.Saga.RejectedPackageIds)
                        .ToList();
                    ctx.Saga.RejectedPackageIds.AddRange(pending);
                    ctx.Saga.RejectedPackages = ctx.Saga.TotalPackages - ctx.Saga.ConfirmedPackages;
                })
                .IfElse(ctx => ctx.Saga.ConfirmedPackages == 0,
                    allExpired => allExpired
                        .Then(ctx =>
                        {
                            ctx.Saga.FailureReason = "Seller confirmation window expired — no packages confirmed";
                            ctx.Saga.FailedAt      = DateTimeOffset.UtcNow;
                        })
                        .TransitionTo(Compensating)
                        .PublishAsync(ctx => ctx.Init<ReleaseInventory>(new
                        {
                            ctx.Saga.CorrelationId,
                            OrderId        = ctx.Saga.CorrelationId,
                            ReservationIds = ctx.Saga.ReservationIds
                        })),
                    partial => partial
                        .PublishAsync(ctx => ctx.Init<ReleaseInventory>(new
                        {
                            ctx.Saga.CorrelationId,
                            OrderId        = ctx.Saga.CorrelationId,
                            ReservationIds = GetRejectedReservationIds(ctx.Saga)
                        }))
                        .TransitionTo(NotifyingCustomer)
                        .PublishAsync(ctx => ctx.Init<NotifyCustomer>(new
                        {
                            ctx.Saga.CorrelationId,
                            OrderId              = ctx.Saga.CorrelationId,
                            UserId               = ctx.Saga.UserId,
                            IsPartialOrder       = true,
                            RejectedPackageCount = ctx.Saga.RejectedPackages,
                            RefundAmount         = 0L
                        }))
                )
        );

        // ── NOTIFYING CUSTOMER ───────────────────────────────────────────────
        During(NotifyingCustomer,
            When(CustomerNotified)
                .Then(ctx => ctx.Saga.CompletedAt = DateTimeOffset.UtcNow)
                .TransitionTo(Completed)
                .Finalize()
        );

        // ── COMPENSATING ─────────────────────────────────────────────────────
        During(Compensating,
            When(InventoryReleased)
                .PublishAsync(ctx => ctx.Init<CancelOrder>(new
                {
                    ctx.Message.CorrelationId,
                    OrderId = ctx.Message.CorrelationId,
                    Reason  = ctx.Saga.FailureReason
                })),

            When(OrderCancelled)
                .Then(ctx =>
                {
                    ctx.Saga.FailedAt ??= DateTimeOffset.UtcNow;
                })
                .TransitionTo(Failed)
                .Finalize()
        );

        SetCompletedWhenFinalized();
    }

    private static bool AllPackagesResolved(FulfillmentSagaState saga) =>
        saga.ConfirmedPackages + saga.RejectedPackages >= saga.TotalPackages;

    private static List<Guid> GetRejectedReservationIds(FulfillmentSagaState saga) =>
        saga.RejectedPackageIds
            .SelectMany(pkgId => saga.PackageReservationMap.TryGetValue(pkgId, out var ids) ? ids : [])
            .ToList();
}
