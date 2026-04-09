using HiveSpace.Core.Contexts;
using HiveSpace.Core.Exceptions;
using HiveSpace.Core.Exceptions.Models;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Commands;
using HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Events;
using HiveSpace.OrderService.Api.Models;
using HiveSpace.OrderService.Application.Cart.Queries.GetCheckoutPreview;
using HiveSpace.OrderService.Application.Orders.Queries.GetCheckoutStatus;
using HiveSpace.OrderService.Domain.Exceptions;
using MassTransit;
using MediatR;

namespace HiveSpace.OrderService.Api.Endpoints;

public static class CheckoutEndpoints
{
    public static IEndpointRouteBuilder MapCheckoutEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/orders/checkout", async (
            CheckoutRequest request,
            IBus bus,
            IUserContext userContext,
            CancellationToken ct) =>
        {
            var checkoutClient = bus.CreateRequestClient<CheckoutInitiated>();
            var correlationId = NewId.NextGuid();
            try
            {
                var response = await checkoutClient.GetResponse<CheckoutResponse, CheckoutFailed>(new
                {
                    CorrelationId = correlationId,
                    userContext.UserId,
                    request.DeliveryAddress,
                    CouponCodes = request.CouponCodes ?? []
                }, ct, RequestTimeout.After(m: 2));

                if (response.Is(out Response<CheckoutResponse>? success) && success != null)
                    return Results.Ok(success.Message);

                if (response.Is(out Response<CheckoutFailed>? failed) && failed != null)
                    throw MapCheckoutFailure(failed.Message);

                throw new DomainException(500, OrderDomainErrorCode.CheckoutInternalError, nameof(CheckoutInitiated));
            }
            catch (RequestTimeoutException)
            {
                throw new DomainException(504, OrderDomainErrorCode.CheckoutTimeout, nameof(CheckoutInitiated));
            }
        })
        .RequireAuthorization()
        .WithName("InitiateCheckout")
        .WithTags("Checkout")
        .WithSummary("Initiate a checkout")
        .WithDescription("Starts the checkout saga for the given cart.");

        app.MapGet("/api/v1/orders/checkout/{orderId:guid}", async (
            Guid orderId,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new GetCheckoutStatusQuery(orderId), ct);
            return Results.Ok(result);
        })
        .RequireAuthorization()
        .WithName("GetCheckoutStatus")
        .WithTags("Checkout")
        .WithSummary("Get checkout status")
        .WithDescription("Returns the checkout saga status by orderId. The orderId is the correlationId returned when checkout is initiated.");

        app.MapPost("/api/v1/orders/checkout/preview", async (
            CheckoutPreviewRequest request,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new GetCheckoutPreviewQuery(
                request.StoreCoupons,
                request.PlatformCouponCodes
            ), ct);
            return Results.Ok(result);
        })
        .RequireAuthorization()
        .WithName("GetCheckoutPreview")
        .WithTags("Checkout")
        .WithSummary("Get checkout preview")
        .WithDescription("Returns a preview of selected cart items grouped by store, with coupon-applied prices and shipping estimates.");

        return app;
    }

    private static Exception MapCheckoutFailure(CheckoutFailed msg) => msg.ErrorType switch
    {
        CheckoutErrorType.ValidationFailed =>
            new BadRequestException(
                msg.Errors.Count > 0
                    ? msg.Errors.Select(e => new Error(OrderDomainErrorCode.CheckoutValidationFailed, e))
                    : [new Error(OrderDomainErrorCode.CheckoutValidationFailed, nameof(CheckoutInitiated))]),

        CheckoutErrorType.InventoryUnavailable =>
            new ConflictException(OrderDomainErrorCode.CheckoutInventoryUnavailable, nameof(CheckoutInitiated)),

        CheckoutErrorType.CODLimitExceeded =>
            new DomainException(422, OrderDomainErrorCode.CheckoutCODLimitExceeded, nameof(CheckoutInitiated)),

        CheckoutErrorType.Timeout =>
            new DomainException(504, OrderDomainErrorCode.CheckoutTimeout, nameof(CheckoutInitiated)),

        _ =>
            new DomainException(500, OrderDomainErrorCode.CheckoutInternalError, nameof(CheckoutInitiated)),
    };
}
