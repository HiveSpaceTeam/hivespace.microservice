using HiveSpace.Core.Contexts;
using HiveSpace.Core.Exceptions;
using HiveSpace.Core.Exceptions.Models;
using HiveSpace.Domain.Shared.Entities;
using HiveSpace.Domain.Shared.Enumerations;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Commands;
using HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Dtos;
using HiveSpace.OrderService.Api.Models;
using HiveSpace.OrderService.Application.Cart;
using HiveSpace.OrderService.Application.Cart.Queries.GetCheckoutPreview;
using HiveSpace.OrderService.Domain.Aggregates.Carts;
using HiveSpace.OrderService.Domain.Exceptions;
using HiveSpace.OrderService.Domain.Repositories;
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
            ICartRepository cartRepository,
            ICheckoutQuery checkoutQuery,
            ICouponRepository couponRepository,
            IUserContext userContext,
            CancellationToken ct) =>
        {
            var cart = await cartRepository.GetByUserIdAsync(userContext.UserId, ct)
                ?? throw new NotFoundException(OrderDomainErrorCode.CartNotFound, nameof(Cart));
            var selectedCart = await checkoutQuery.GetSelectedCartItemsAsync(userContext.UserId, ct);
            SelectedCartCouponEvaluator.EnsureSelectedCartExists(selectedCart, nameof(CheckoutInitiated));
            var couponState = await PersistedCartCouponState.ValidateAsync(
                cart,
                SelectedCartCouponEvaluator.BuildStoreSnapshots(selectedCart),
                couponRepository,
                userContext.UserId,
                ct,
                removeInvalidSelections: false);
            if (couponState.InvalidatedCoupons.Count > 0)
                throw PersistedCartCouponState.BuildCheckoutCouponException(couponState.InvalidatedCoupons);

            var paymentMethod = Enumeration.FromValue<PaymentMethod>(request.PaymentMethod ?? PaymentMethod.COD.Id);
            var couponSelections = new CheckoutCouponSelectionDto
            {
                PlatformCouponCodes = couponState.AppliedPlatformCoupons
                    .Select(x => x.CouponCode)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList(),
                StoreCoupons = couponState.AppliedStoreCoupons.Values
                    .Select(x => new StoreCouponSelectionDto(x.StoreId, x.CouponCode))
                    .ToList()
            };

            var checkoutClient = bus.CreateRequestClient<CheckoutInitiated>();
            var correlationId = NewId.NextGuid();
            try
            {
                var response = await checkoutClient.GetResponse<CheckoutResponse, CheckoutFailed>(new
                {
                    CorrelationId = correlationId,
                    userContext.UserId,
                    request.DeliveryAddress,
                    CouponSelections = couponSelections,
                    PaymentMethod = paymentMethod
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

        app.MapPost("/api/v1/orders/checkout/preview", async (
            CheckoutPreviewRequest request,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new GetCheckoutPreviewQuery(), ct);
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

        CheckoutErrorType.PaymentFailed =>
            new DomainException(422, OrderDomainErrorCode.CheckoutPaymentFailed, nameof(CheckoutInitiated)),

        CheckoutErrorType.Timeout =>
            new DomainException(504, OrderDomainErrorCode.CheckoutTimeout, nameof(CheckoutInitiated)),

        _ =>
            new DomainException(500, OrderDomainErrorCode.CheckoutInternalError, nameof(CheckoutInitiated)),
    };
}
