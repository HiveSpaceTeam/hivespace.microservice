using HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Commands;
using HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Events;
using HiveSpace.OrderService.Infrastructure.Data;
using HiveSpace.OrderService.Infrastructure.Sagas;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace HiveSpace.OrderService.Api.Sagas.FulfillmentSaga;

public class FulfillmentSagaStateMachine : MassTransitStateMachine<FulfillmentSagaState>
{
    // ── Request (sync — consumer must RespondAsync) ───────────────────────────
    public Request<FulfillmentSagaState, ConfirmInventory, InventoryConfirmed, InventoryConfirmationFailed>
        InventoryConfirmation { get; private set; } = null!;

    // ── Scheduled timeout (3-day seller confirmation window) ─────────────────
    public Schedule<FulfillmentSagaState, SellerConfirmationExpired> SellerConfirmationTimeout { get; private set; } = null!;

    // ── Events (pub/sub) ──────────────────────────────────────────────────────
    public Event<OrderReadyForFulfillment> OrderReadyForFulfillment { get; private set; } = null!;
    public Event<SellerNewOrderNotified>   SellerNewOrderNotified   { get; private set; } = null!;
    public Event<OrderConfirmedBySeller>   OrderConfirmedBySeller   { get; private set; } = null!;
    public Event<OrderRejectedBySeller>    OrderRejectedBySeller    { get; private set; } = null!;
    public Event<BuyerNotified>         BuyerNotified         { get; private set; } = null!;
    public Event<InventoryReleased>        InventoryReleased        { get; private set; } = null!;
    public Event<OrderCancelled>           OrderCancelled           { get; private set; } = null!;

    // ── States ────────────────────────────────────────────────────────────────
    public State ConfirmingInventory           { get; private set; } = null!;
    public State NotifyingSeller               { get; private set; } = null!;
    public State WaitingForSellerConfirmation  { get; private set; } = null!;
    public State NotifyingBuyer             { get; private set; } = null!;
    public State Compensating                  { get; private set; } = null!;
    public State Completed                     { get; private set; } = null!;
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

        Event(() => OrderReadyForFulfillment, x => x.CorrelateById(m => m.Message.CorrelationId));
        Event(() => SellerNewOrderNotified,   x => x.CorrelateById(m => m.Message.CorrelationId));
        Event(() => OrderConfirmedBySeller,   x => x.CorrelateById(m => m.Message.CorrelationId));
        Event(() => OrderRejectedBySeller,    x => x.CorrelateById(m => m.Message.CorrelationId));
        Event(() => BuyerNotified,         x => x.CorrelateById(m => m.Message.CorrelationId));
        Event(() => InventoryReleased,        x => x.CorrelateById(m => m.Message.CorrelationId));
        Event(() => OrderCancelled,           x => x.CorrelateById(m => m.Message.CorrelationId));

        // ── INITIAL ──────────────────────────────────────────────────────────
        Initially(
            When(OrderReadyForFulfillment)
                .Then(ctx =>
                {
                    ctx.Saga.UserId         = ctx.Message.UserId;
                    ctx.Saga.StoreId        = ctx.Message.StoreId;
                    ctx.Saga.OrderCode      = ctx.Message.OrderCode;
                    ctx.Saga.ReservationIds = ctx.Message.ReservationIds;
                    ctx.Saga.GrandTotal     = ctx.Message.GrandTotal;
                    ctx.Saga.PaymentMethod  = ctx.Message.PaymentMethod;
                    ctx.Saga.CreatedAt      = DateTimeOffset.UtcNow;
                })
                .TransitionTo(NotifyingSeller)
                .PublishAsync(async ctx =>
                {
                    var db = ctx.GetPayload<IServiceProvider>()
                                .GetRequiredService<OrderDbContext>();
                    var storeRef = await db.StoreRefs
                        .FirstOrDefaultAsync(s => s.Id == ctx.Saga.StoreId);

                    return await ctx.Init<NotifySellerNewOrder>(new
                    {
                        ctx.Saga.CorrelationId,
                        OrderId   = ctx.Saga.CorrelationId,
                        ctx.Saga.StoreId,
                        SellerId  = storeRef?.OwnerId ?? Guid.Empty,
                        BuyerId   = ctx.Saga.UserId,
                        ctx.Saga.OrderCode
                    });
                })
        );

        // ── CONFIRMING INVENTORY (after seller confirms) ─────────────────────
        During(InventoryConfirmation.Pending,
            When(InventoryConfirmation.Completed)   // InventoryConfirmed
                .TransitionTo(NotifyingBuyer)
                .PublishAsync(ctx => ctx.Init<NotifyBuyerOrderConfirmed>(new
                {
                    ctx.Saga.CorrelationId,
                    OrderId  = ctx.Saga.CorrelationId,
                    BuyerId  = ctx.Saga.UserId,
                    ctx.Saga.StoreId,
                    ctx.Saga.OrderCode
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

        // ── NOTIFYING SELLER ─────────────────────────────────────────────────
        During(NotifyingSeller,
            When(SellerNewOrderNotified)
                .Schedule(SellerConfirmationTimeout, ctx => ctx.Init<SellerConfirmationExpired>(new
                {
                    ctx.Saga.CorrelationId,
                    OrderId = ctx.Saga.CorrelationId
                }))
                .TransitionTo(WaitingForSellerConfirmation)
        );

        // ── WAITING FOR SELLER CONFIRMATION ──────────────────────────────────
        During(WaitingForSellerConfirmation,
            When(OrderConfirmedBySeller)
                .Then(ctx => ctx.Saga.OrderWasConfirmed = true)
                .Unschedule(SellerConfirmationTimeout)
                .Request(InventoryConfirmation, ctx => ctx.Init<ConfirmInventory>(new
                {
                    ctx.Saga.CorrelationId,
                    OrderId        = ctx.Saga.CorrelationId,
                    ReservationIds = ctx.Saga.ReservationIds
                }))
                .TransitionTo(InventoryConfirmation.Pending),

            When(OrderRejectedBySeller)
                .Then(ctx =>
                {
                    ctx.Saga.FailureReason = ctx.Message.Reason;
                    ctx.Saga.FailedAt      = DateTimeOffset.UtcNow;
                })
                .Unschedule(SellerConfirmationTimeout)
                .TransitionTo(Compensating)
                .PublishAsync(ctx => ctx.Init<ReleaseInventory>(new
                {
                    ctx.Saga.CorrelationId,
                    OrderId        = ctx.Saga.CorrelationId,
                    ReservationIds = ctx.Saga.ReservationIds
                })),

            When(SellerConfirmationTimeout.Received)
                .Then(ctx =>
                {
                    ctx.Saga.FailureReason = "Seller did not respond within the confirmation window";
                    ctx.Saga.FailedAt      = DateTimeOffset.UtcNow;
                })
                .TransitionTo(Compensating)
                .PublishAsync(ctx => ctx.Init<ReleaseInventory>(new
                {
                    ctx.Saga.CorrelationId,
                    OrderId        = ctx.Saga.CorrelationId,
                    ReservationIds = ctx.Saga.ReservationIds
                }))
        );

        // ── NOTIFYING CUSTOMER ───────────────────────────────────────────────
        During(NotifyingBuyer,
            When(BuyerNotified)
                .IfElse(ctx => ctx.Saga.OrderWasConfirmed,
                    success => success
                        .Then(ctx => ctx.Saga.CompletedAt = DateTimeOffset.UtcNow)
                        .TransitionTo(Completed)
                        .Finalize(),
                    failure => failure
                        .Then(ctx => ctx.Saga.FailedAt ??= DateTimeOffset.UtcNow)
                        .TransitionTo(Failed)
                        .Finalize()
                )
        );

        // ── COMPENSATING ─────────────────────────────────────────────────────
        During(Compensating,
            When(InventoryReleased)
                .PublishAsync(ctx => ctx.Init<CancelOrder>(new
                {
                    ctx.Saga.CorrelationId,
                    OrderId = ctx.Saga.CorrelationId,
                    Reason  = ctx.Saga.FailureReason
                })),

            When(OrderCancelled)
                .TransitionTo(NotifyingBuyer)
                .PublishAsync(ctx => ctx.Init<NotifyBuyerOrderCancelled>(new
                {
                    ctx.Saga.CorrelationId,
                    OrderId      = ctx.Saga.CorrelationId,
                    BuyerId      = ctx.Saga.UserId,
                    RefundAmount = 0L,
                    ctx.Saga.OrderCode
                }))
        );

        SetCompletedWhenFinalized();
    }
}
