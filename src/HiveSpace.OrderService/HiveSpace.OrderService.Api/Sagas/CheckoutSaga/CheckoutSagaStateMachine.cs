using HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Commands;
using HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Events;
using HiveSpace.OrderService.Api.Models;
using HiveSpace.OrderService.Domain.Enumerations;
using HiveSpace.OrderService.Infrastructure.Sagas;
using MassTransit;

namespace HiveSpace.OrderService.Api.Sagas.CheckoutSaga;

public class CheckoutSagaStateMachine : MassTransitStateMachine<CheckoutSagaState>
{
    // ── Requests (sync steps — consumers must RespondAsync) ──────────────────
    public Request<CheckoutSagaState, ValidateCheckout, ValidationCompleted, ValidationFailed>         CartValidation       { get; private set; } = null!;
    public Request<CheckoutSagaState, CreateOrder, OrderCreated>                                        OrderCreation        { get; private set; } = null!;
    public Request<CheckoutSagaState, ReserveInventory, InventoryReserved, InventoryReservationFailed> InventoryReservation { get; private set; } = null!;
    public Request<CheckoutSagaState, MarkOrderAsCOD, OrderMarkedAsCOD, MarkOrderAsCODFailed>          CODMarking           { get; private set; } = null!;

    // ── Events (compensation — still pub/sub) ────────────────────────────────
    public Event<CheckoutInitiated> CheckoutInitiated { get; private set; } = null!;
    public Event<InventoryReleased> InventoryReleased { get; private set; } = null!;
    public Event<OrderCancelled>    OrderCancelled    { get; private set; } = null!;

    // ── States ────────────────────────────────────────────────────────────────
    public State Compensating { get; private set; } = null!;
    public State Completed    { get; private set; } = null!;
    public State Failed       { get; private set; } = null!;

    public CheckoutSagaStateMachine()
    {
        InstanceState(x => x.CurrentState);

        // Configure requests — each has its own 30-min timeout
        Request(() => CartValidation,       x => x.CartValidationPendingTokenId,       cfg => cfg.Timeout = TimeSpan.FromMinutes(30));
        Request(() => OrderCreation,        x => x.OrderCreationPendingTokenId,        cfg => cfg.Timeout = TimeSpan.FromMinutes(30));
        Request(() => InventoryReservation, x => x.InventoryReservationPendingTokenId, cfg => cfg.Timeout = TimeSpan.FromMinutes(30));
        Request(() => CODMarking,           x => x.CODMarkingPendingTokenId,           cfg => cfg.Timeout = TimeSpan.FromMinutes(30));

        Event(() => CheckoutInitiated, x => x.CorrelateById(m => m.Message.CorrelationId));
        Event(() => InventoryReleased,  x => x.CorrelateById(m => m.Message.CorrelationId));
        Event(() => OrderCancelled,     x => x.CorrelateById(m => m.Message.CorrelationId));

        // ── INITIAL ──────────────────────────────────────────────────────────
        // After SetCompletedWhenFinalized() removes the saga row, the outbox may
        // re-deliver responses or timeouts. MassTransit then creates a ghost
        // Initial instance — finalize immediately to delete it and stop retries.
        Initially(
            When(CartValidation.Completed).Finalize(),
            When(CartValidation.Completed2).Finalize(),
            When(CartValidation.Faulted).Finalize(),
            When(CartValidation.TimeoutExpired).Finalize(),
            When(OrderCreation.Completed).Finalize(),
            When(OrderCreation.Faulted).Finalize(),
            When(OrderCreation.TimeoutExpired).Finalize(),
            When(InventoryReservation.Completed).Finalize(),
            When(InventoryReservation.Completed2).Finalize(),
            When(InventoryReservation.Faulted).Finalize(),
            When(InventoryReservation.TimeoutExpired).Finalize(),
            When(CODMarking.Completed).Finalize(),
            When(CODMarking.Completed2).Finalize(),
            When(CODMarking.Faulted).Finalize(),
            When(CODMarking.TimeoutExpired).Finalize(),

            When(CheckoutInitiated)
                .Then(ctx =>
                {
                    ctx.Saga.RequestId       = ctx.RequestId;
                    ctx.Saga.ResponseAddress = ctx.ResponseAddress;
                    ctx.Saga.UserId          = ctx.Message.UserId;
                    ctx.Saga.DeliveryAddress = ctx.Message.DeliveryAddress;
                    ctx.Saga.CouponCodes     = ctx.Message.CouponCodes;
                    ctx.Saga.PaymentMethod   = ctx.Message.PaymentMethod;
                    ctx.Saga.CreatedAt       = DateTimeOffset.UtcNow;
                })
                .Request(CartValidation, ctx => ctx.Init<ValidateCheckout>(new
                {
                    ctx.Saga.CorrelationId,
                    ctx.Saga.UserId,
                    ctx.Saga.CouponCodes,
                    ctx.Saga.DeliveryAddress
                }))
                .TransitionTo(CartValidation.Pending)
        );

        // ── CART VALIDATION ───────────────────────────────────────────────────
        During(CartValidation.Pending,
            When(CartValidation.Completed)
                .Then(ctx =>
                {
                    ctx.Saga.Items          = ctx.Message.Items;
                    ctx.Saga.Subtotal       = ctx.Message.Subtotal;
                    ctx.Saga.ShippingFee    = ctx.Message.ShippingFee;
                    ctx.Saga.TaxAmount      = ctx.Message.TaxAmount;
                    ctx.Saga.DiscountAmount = ctx.Message.DiscountAmount;
                    ctx.Saga.GrandTotal     = ctx.Message.GrandTotal;
                })
                .Request(OrderCreation, ctx => ctx.Init<CreateOrder>(new
                {
                    ctx.Saga.CorrelationId,
                    UserId          = ctx.Saga.UserId,
                    Items           = ctx.Saga.Items,
                    DeliveryAddress = ctx.Saga.DeliveryAddress,
                    Subtotal        = ctx.Saga.Subtotal,
                    ShippingFee     = ctx.Saga.ShippingFee,
                    TaxAmount       = ctx.Saga.TaxAmount,
                    DiscountAmount  = ctx.Saga.DiscountAmount,
                    GrandTotal      = ctx.Saga.GrandTotal,
                    PaymentMethod   = ctx.Saga.PaymentMethod
                }))
                .TransitionTo(OrderCreation.Pending),

            When(CartValidation.Completed2)   // ValidationFailed — no compensation needed
                .Then(ctx =>
                {
                    ctx.Saga.FailureReason = ctx.Message.Reason;
                    ctx.Saga.FailedAt      = DateTimeOffset.UtcNow;
                })
                .ThenAsync(ctx => RespondWithFailure(ctx,
                    ctx.Saga.RequestId, ctx.Saga.ResponseAddress,
                    CheckoutErrorType.ValidationFailed, ctx.Saga.FailureReason!, ctx.Message.Errors))
                .TransitionTo(Failed)
                .Finalize(),

            When(CartValidation.Faulted)
                .Then(ctx =>
                {
                    ctx.Saga.FailureReason = "Unexpected error during checkout validation";
                    ctx.Saga.FailedAt      = DateTimeOffset.UtcNow;
                })
                .ThenAsync(ctx => RespondWithFailure(ctx,
                    ctx.Saga.RequestId, ctx.Saga.ResponseAddress,
                    CheckoutErrorType.InternalError, ctx.Saga.FailureReason!))
                .TransitionTo(Failed)
                .Finalize(),

            When(CartValidation.TimeoutExpired)
                .Then(ctx =>
                {
                    ctx.Saga.FailureReason = "Checkout validation timed out";
                    ctx.Saga.FailedAt      = DateTimeOffset.UtcNow;
                })
                .ThenAsync(ctx => RespondWithFailure(ctx,
                    ctx.Saga.RequestId, ctx.Saga.ResponseAddress,
                    CheckoutErrorType.Timeout, ctx.Saga.FailureReason!))
                .TransitionTo(Failed)
                .Finalize()
        );

        // ── ORDER CREATION ────────────────────────────────────────────────────
        During(OrderCreation.Pending,
            When(OrderCreation.Completed)
                .Then(ctx =>
                {
                    ctx.Saga.PackageIds = ctx.Message.PackageIds;
                    // CorrelationId IS the OrderId — CreateOrderConsumer uses CorrelationId as Order.Id
                })
                .Request(InventoryReservation, ctx => ctx.Init<ReserveInventory>(new
                {
                    ctx.Saga.CorrelationId,
                    OrderId           = ctx.Saga.CorrelationId,
                    Items             = ctx.Saga.Items,
                    ExpirationMinutes = 15
                }))
                .TransitionTo(InventoryReservation.Pending),

            When(OrderCreation.Faulted)
                .Then(ctx =>
                {
                    ctx.Saga.FailureReason = "Unexpected error during order creation";
                    ctx.Saga.FailedAt      = DateTimeOffset.UtcNow;
                })
                .ThenAsync(ctx => RespondWithFailure(ctx,
                    ctx.Saga.RequestId, ctx.Saga.ResponseAddress,
                    CheckoutErrorType.InternalError, ctx.Saga.FailureReason!))
                .TransitionTo(Compensating)
                .PublishAsync(ctx => ctx.Init<CancelOrder>(new
                {
                    ctx.Saga.CorrelationId,
                    OrderId = ctx.Saga.CorrelationId,
                    Reason  = ctx.Saga.FailureReason
                })),

            When(OrderCreation.TimeoutExpired)
                .Then(ctx =>
                {
                    ctx.Saga.FailureReason = "Order creation timed out";
                    ctx.Saga.FailedAt      = DateTimeOffset.UtcNow;
                })
                .ThenAsync(ctx => RespondWithFailure(ctx,
                    ctx.Saga.RequestId, ctx.Saga.ResponseAddress,
                    CheckoutErrorType.Timeout, ctx.Saga.FailureReason!))
                .TransitionTo(Compensating)
                .PublishAsync(ctx => ctx.Init<CancelOrder>(new
                {
                    ctx.Saga.CorrelationId,
                    OrderId = ctx.Saga.CorrelationId,
                    Reason  = ctx.Saga.FailureReason
                }))
        );

        // ── INVENTORY RESERVATION ─────────────────────────────────────────────
        During(InventoryReservation.Pending,
            When(InventoryReservation.Completed)
                .Then(ctx =>
                {
                    ctx.Saga.ReservationIds        = ctx.Message.ReservationIds;
                    ctx.Saga.PackageReservationMap = ctx.Message.PackageReservationMap;
                })
                .Request(CODMarking, ctx => ctx.Init<MarkOrderAsCOD>(new
                {
                    ctx.Saga.CorrelationId,
                    OrderId = ctx.Saga.CorrelationId
                }))
                .TransitionTo(CODMarking.Pending),

            When(InventoryReservation.Completed2)   // InventoryReservationFailed
                .Then(ctx =>
                {
                    ctx.Saga.FailureReason = ctx.Message.Reason;
                    ctx.Saga.FailedAt      = DateTimeOffset.UtcNow;
                })
                .ThenAsync(ctx => RespondWithFailure(ctx,
                    ctx.Saga.RequestId, ctx.Saga.ResponseAddress,
                    CheckoutErrorType.InventoryUnavailable, ctx.Saga.FailureReason!))
                .TransitionTo(Compensating)
                .PublishAsync(ctx => ctx.Init<CancelOrder>(new
                {
                    ctx.Saga.CorrelationId,
                    OrderId = ctx.Saga.CorrelationId,
                    Reason  = ctx.Saga.FailureReason
                })),

            When(InventoryReservation.Faulted)
                .Then(ctx =>
                {
                    ctx.Saga.FailureReason = "Unexpected error during inventory reservation";
                    ctx.Saga.FailedAt      = DateTimeOffset.UtcNow;
                })
                .ThenAsync(ctx => RespondWithFailure(ctx,
                    ctx.Saga.RequestId, ctx.Saga.ResponseAddress,
                    CheckoutErrorType.InternalError, ctx.Saga.FailureReason!))
                .TransitionTo(Compensating)
                .PublishAsync(ctx => ctx.Init<CancelOrder>(new
                {
                    ctx.Saga.CorrelationId,
                    OrderId = ctx.Saga.CorrelationId,
                    Reason  = ctx.Saga.FailureReason
                })),

            When(InventoryReservation.TimeoutExpired)
                .Then(ctx =>
                {
                    ctx.Saga.FailureReason = "Inventory reservation timed out";
                    ctx.Saga.FailedAt      = DateTimeOffset.UtcNow;
                })
                .ThenAsync(ctx => RespondWithFailure(ctx,
                    ctx.Saga.RequestId, ctx.Saga.ResponseAddress,
                    CheckoutErrorType.Timeout, ctx.Saga.FailureReason!))
                .TransitionTo(Compensating)
                .PublishAsync(ctx => ctx.Init<CancelOrder>(new
                {
                    ctx.Saga.CorrelationId,
                    OrderId = ctx.Saga.CorrelationId,
                    Reason  = ctx.Saga.FailureReason
                }))
        );

        // ── COD MARKING ───────────────────────────────────────────────────────
        During(CODMarking.Pending,
            When(CODMarking.Completed)   // SUCCESS — respond to API, hand off to FulfillmentSaga
                .ThenAsync(async ctx =>
                {
                    ctx.Saga.CompletedAt = DateTimeOffset.UtcNow;
                    if (ctx.Saga.ResponseAddress is not null)
                    {
                        var ep = await ctx.GetSendEndpoint(ctx.Saga.ResponseAddress);
                        await ep.Send(new CheckoutResponse
                        {
                            OrderId    = ctx.Saga.CorrelationId,
                            Status     = OrderStatus.COD.Name,
                            GrandTotal = ctx.Saga.GrandTotal
                        }, x => x.RequestId = ctx.Saga.RequestId);
                    }
                })
                .PublishAsync(ctx => ctx.Init<CheckoutPaymentSettled>(new
                {
                    CorrelationId        = ctx.Saga.CorrelationId,
                    ctx.Saga.UserId,
                    ctx.Saga.PackageIds,
                    ctx.Saga.ReservationIds,
                    ctx.Saga.PackageReservationMap,
                    ctx.Saga.GrandTotal
                }))
                .TransitionTo(Completed)
                .Finalize(),

            When(CODMarking.Completed2)   // MarkOrderAsCODFailed — release inventory then cancel
                .Then(ctx =>
                {
                    ctx.Saga.FailureReason = ctx.Message.Reason;
                    ctx.Saga.FailedAt      = DateTimeOffset.UtcNow;
                })
                .ThenAsync(ctx => RespondWithFailure(ctx,
                    ctx.Saga.RequestId, ctx.Saga.ResponseAddress,
                    CheckoutErrorType.CODLimitExceeded, ctx.Saga.FailureReason!))
                .TransitionTo(Compensating)
                .PublishAsync(ctx => ctx.Init<ReleaseInventory>(new
                {
                    ctx.Saga.CorrelationId,
                    OrderId        = ctx.Saga.CorrelationId,
                    ReservationIds = ctx.Saga.ReservationIds
                })),

            When(CODMarking.Faulted)
                .Then(ctx =>
                {
                    ctx.Saga.FailureReason = "Unexpected error during COD marking";
                    ctx.Saga.FailedAt      = DateTimeOffset.UtcNow;
                })
                .ThenAsync(ctx => RespondWithFailure(ctx,
                    ctx.Saga.RequestId, ctx.Saga.ResponseAddress,
                    CheckoutErrorType.InternalError, ctx.Saga.FailureReason!))
                .TransitionTo(Compensating)
                .PublishAsync(ctx => ctx.Init<ReleaseInventory>(new
                {
                    ctx.Saga.CorrelationId,
                    OrderId        = ctx.Saga.CorrelationId,
                    ReservationIds = ctx.Saga.ReservationIds
                })),

            When(CODMarking.TimeoutExpired)
                .Then(ctx =>
                {
                    ctx.Saga.FailureReason = "COD marking timed out";
                    ctx.Saga.FailedAt      = DateTimeOffset.UtcNow;
                })
                .ThenAsync(ctx => RespondWithFailure(ctx,
                    ctx.Saga.RequestId, ctx.Saga.ResponseAddress,
                    CheckoutErrorType.Timeout, ctx.Saga.FailureReason!))
                .TransitionTo(Compensating)
                .PublishAsync(ctx => ctx.Init<ReleaseInventory>(new
                {
                    ctx.Saga.CorrelationId,
                    OrderId        = ctx.Saga.CorrelationId,
                    ReservationIds = ctx.Saga.ReservationIds
                }))
        );

        // ── COMPENSATING ──────────────────────────────────────────────────────
        During(Compensating,
            When(CartValidation.Completed).Then(_ => {}),
            When(CartValidation.Completed2).Then(_ => {}),
            When(CartValidation.Faulted).Then(_ => {}),
            When(CartValidation.TimeoutExpired).Then(_ => {}),
            When(OrderCreation.Completed).Then(_ => {}),
            When(OrderCreation.Faulted).Then(_ => {}),
            When(OrderCreation.TimeoutExpired).Then(_ => {}),
            When(InventoryReservation.Completed).Then(_ => {}),
            When(InventoryReservation.Completed2).Then(_ => {}),
            When(InventoryReservation.Faulted).Then(_ => {}),
            When(InventoryReservation.TimeoutExpired).Then(_ => {}),
            When(CODMarking.Completed).Then(_ => {}),
            When(CODMarking.Completed2).Then(_ => {}),
            When(CODMarking.Faulted).Then(_ => {}),
            When(CODMarking.TimeoutExpired).Then(_ => {}),

            When(InventoryReleased)
                .PublishAsync(ctx => ctx.Init<CancelOrder>(new
                {
                    ctx.Message.CorrelationId,
                    OrderId = ctx.Message.CorrelationId,
                    Reason  = ctx.Saga.FailureReason
                })),

            When(OrderCancelled)
                .TransitionTo(Failed)
                .Finalize()
        );

        SetCompletedWhenFinalized();
    }

    private static async Task RespondWithFailure(
        ISendEndpointProvider sendEndpointProvider,
        Guid? requestId,
        Uri? responseAddress,
        CheckoutErrorType errorType,
        string reason,
        IEnumerable<string>? errors = null)
    {
        if (responseAddress is null) return;
        var ep = await sendEndpointProvider.GetSendEndpoint(responseAddress);
        await ep.Send(new CheckoutFailed
        {
            Reason    = reason,
            Errors    = errors?.ToList() ?? [],
            ErrorType = errorType
        }, x => x.RequestId = requestId);
    }
}
