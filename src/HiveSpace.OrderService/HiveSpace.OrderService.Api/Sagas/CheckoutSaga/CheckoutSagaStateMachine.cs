using HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Commands;
using HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Events;
using HiveSpace.OrderService.Api.Models;
using HiveSpace.OrderService.Application.Contracts;
using HiveSpace.OrderService.Domain.Enumerations;
using HiveSpace.OrderService.Infrastructure.Sagas;
using MassTransit;

namespace HiveSpace.OrderService.Api.Sagas.CheckoutSaga;

public class CheckoutSagaStateMachine : MassTransitStateMachine<CheckoutSagaState>
{
    // ── Requests (sync steps) ────────────────────────────────────────────────
    public Request<CheckoutSagaState, CreateOrder, OrderCreated, OrderCreationFailed>                    OrderCreation        { get; private set; } = null!;
    public Request<CheckoutSagaState, ReserveInventory, InventoryReserved, InventoryReservationFailed>   InventoryReservation { get; private set; } = null!;
    public Request<CheckoutSagaState, MarkOrderAsCOD, OrderMarkedAsCOD, MarkOrderAsCODFailed>            CODMarking           { get; private set; } = null!;
    public Request<CheckoutSagaState, ClearCart, CartCleared>                                            CartClearing         { get; private set; } = null!;

    // ── Events (compensation) ────────────────────────────────────────────────
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

        Request(() => OrderCreation,        x => x.OrderCreationPendingTokenId,        cfg => cfg.Timeout = TimeSpan.FromMinutes(30));
        Request(() => InventoryReservation, x => x.InventoryReservationPendingTokenId, cfg => cfg.Timeout = TimeSpan.FromMinutes(30));
        Request(() => CODMarking,           x => x.CODMarkingPendingTokenId,           cfg => cfg.Timeout = TimeSpan.FromMinutes(30));
        Request(() => CartClearing,         x => x.CartClearingPendingTokenId,         cfg => cfg.Timeout = TimeSpan.FromMinutes(5));

        Event(() => CheckoutInitiated, x => x.CorrelateById(m => m.Message.CorrelationId));
        Event(() => InventoryReleased,  x => x.CorrelateById(m => m.Message.CorrelationId));
        Event(() => OrderCancelled,     x => x.CorrelateById(m => m.Message.CorrelationId));

        // ── INITIAL ──────────────────────────────────────────────────────────
        Initially(
            // Ghost-instance cleanup after finalization
            When(OrderCreation.Completed).Finalize(),
            When(OrderCreation.Completed2).Finalize(),
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
            When(CartClearing.Completed).Finalize(),
            When(CartClearing.Faulted).Finalize(),
            When(CartClearing.TimeoutExpired).Finalize(),

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
                .Request(OrderCreation, ctx => ctx.Init<CreateOrder>(new
                {
                    ctx.Saga.CorrelationId,
                    ctx.Saga.UserId,
                    ctx.Saga.DeliveryAddress,
                    ctx.Saga.PaymentMethod,
                    ctx.Saga.CouponCodes
                }))
                .TransitionTo(OrderCreation.Pending)
        );

        // ── ORDER CREATION ────────────────────────────────────────────────────
        During(OrderCreation.Pending,
            When(OrderCreation.Completed)
                .Then(ctx =>
                {
                    ctx.Saga.OrderIds      = ctx.Message.OrderIds;
                    ctx.Saga.OrderStoreMap = ctx.Message.OrderStoreMap;
                    ctx.Saga.GrandTotal    = ctx.Message.GrandTotal;
                })
                .Request(InventoryReservation, ctx => ctx.Init<ReserveInventory>(new
                {
                    ctx.Saga.CorrelationId,
                    OrderIds          = ctx.Saga.OrderIds,
                    Items             = ctx.Message.Items,
                    ExpirationMinutes = 15
                }))
                .TransitionTo(InventoryReservation.Pending),

            When(OrderCreation.Completed2)   // OrderCreationFailed
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

            When(OrderCreation.Faulted)
                .Then(ctx =>
                {
                    ctx.Saga.FailureReason = "Unexpected error during order creation";
                    ctx.Saga.FailedAt      = DateTimeOffset.UtcNow;
                })
                .ThenAsync(ctx => RespondWithFailure(ctx,
                    ctx.Saga.RequestId, ctx.Saga.ResponseAddress,
                    CheckoutErrorType.InternalError, ctx.Saga.FailureReason!))
                .TransitionTo(Failed)
                .Finalize(),

            When(OrderCreation.TimeoutExpired)
                .Then(ctx =>
                {
                    ctx.Saga.FailureReason = "Order creation timed out";
                    ctx.Saga.FailedAt      = DateTimeOffset.UtcNow;
                })
                .ThenAsync(ctx => RespondWithFailure(ctx,
                    ctx.Saga.RequestId, ctx.Saga.ResponseAddress,
                    CheckoutErrorType.Timeout, ctx.Saga.FailureReason!))
                .TransitionTo(Failed)
                .Finalize()
        );

        // ── INVENTORY RESERVATION ─────────────────────────────────────────────
        During(InventoryReservation.Pending,
            When(InventoryReservation.Completed)
                .Then(ctx =>
                {
                    ctx.Saga.ReservationIds      = ctx.Message.ReservationIds;
                    ctx.Saga.OrderReservationMap = ctx.Message.OrderReservationMap;
                })
                .Request(CODMarking, ctx => ctx.Init<MarkOrderAsCOD>(new
                {
                    ctx.Saga.CorrelationId,
                    OrderIds = ctx.Saga.OrderIds
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
            When(CODMarking.Completed)
                .Request(CartClearing, ctx => ctx.Init<ClearCart>(new
                {
                    ctx.Saga.CorrelationId,
                    ctx.Saga.UserId
                }))
                .TransitionTo(CartClearing.Pending),

            When(CODMarking.Completed2)   // MarkOrderAsCODFailed
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

        // ── CART CLEARING ─────────────────────────────────────────────────────
        During(CartClearing.Pending,
            When(CartClearing.Completed)   // CartCleared — respond to API and fan-out to FulfillmentSagas
                .ThenAsync(async ctx =>
                {
                    ctx.Saga.CompletedAt = DateTimeOffset.UtcNow;

                    if (ctx.Saga.ResponseAddress is not null)
                    {
                        var ep = await ctx.GetSendEndpoint(ctx.Saga.ResponseAddress);
                        await ep.Send(new CheckoutResponse
                        {
                            OrderIds   = ctx.Saga.OrderIds,
                            Status     = OrderStatus.COD.Name,
                            GrandTotal = ctx.Saga.GrandTotal
                        }, x => x.RequestId = ctx.Saga.RequestId);
                    }

                    // Fan-out: one OrderReadyForFulfillment per order
                    foreach (var orderId in ctx.Saga.OrderIds)
                    {
                        ctx.Saga.OrderReservationMap.TryGetValue(orderId, out var reservations);
                        ctx.Saga.OrderStoreMap.TryGetValue(orderId, out var storeId);
                        await ctx.Publish<OrderReadyForFulfillment>(new
                        {
                            CorrelationId  = orderId,
                            ctx.Saga.UserId,
                            StoreId        = storeId,
                            ReservationIds = reservations ?? new List<Guid>(),
                            ctx.Saga.GrandTotal
                        });
                    }
                })
                .TransitionTo(Completed)
                .Finalize(),

            When(CartClearing.Faulted)
                .Then(ctx =>
                {
                    ctx.Saga.FailureReason = "Cart clearing failed unexpectedly";
                    ctx.Saga.FailedAt      = DateTimeOffset.UtcNow;
                })
                // Cart clearing failure is non-critical — still respond with success
                .ThenAsync(async ctx =>
                {
                    ctx.Saga.CompletedAt = DateTimeOffset.UtcNow;
                    if (ctx.Saga.ResponseAddress is not null)
                    {
                        var ep = await ctx.GetSendEndpoint(ctx.Saga.ResponseAddress);
                        await ep.Send(new CheckoutResponse
                        {
                            OrderIds   = ctx.Saga.OrderIds,
                            Status     = OrderStatus.COD.Name,
                            GrandTotal = ctx.Saga.GrandTotal
                        }, x => x.RequestId = ctx.Saga.RequestId);
                    }

                    foreach (var orderId in ctx.Saga.OrderIds)
                    {
                        ctx.Saga.OrderReservationMap.TryGetValue(orderId, out var reservations);
                        await ctx.Publish<OrderReadyForFulfillment>(new
                        {
                            CorrelationId  = orderId,
                            ctx.Saga.UserId,
                            ReservationIds = reservations ?? new List<Guid>(),
                            ctx.Saga.GrandTotal
                        });
                    }
                })
                .TransitionTo(Completed)
                .Finalize(),

            When(CartClearing.TimeoutExpired)
                .Then(ctx =>
                {
                    ctx.Saga.FailureReason = "Cart clearing timed out";
                    ctx.Saga.FailedAt      = DateTimeOffset.UtcNow;
                })
                .ThenAsync(ctx => RespondWithFailure(ctx,
                    ctx.Saga.RequestId, ctx.Saga.ResponseAddress,
                    CheckoutErrorType.Timeout, ctx.Saga.FailureReason!))
                .TransitionTo(Failed)
                .Finalize()
        );

        // ── COMPENSATING ──────────────────────────────────────────────────────
        During(Compensating,
            When(OrderCreation.Completed).Then(_ => {}),
            When(OrderCreation.Completed2).Then(_ => {}),
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
            When(CartClearing.Completed).Then(_ => {}),
            When(CartClearing.Faulted).Then(_ => {}),
            When(CartClearing.TimeoutExpired).Then(_ => {}),

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
