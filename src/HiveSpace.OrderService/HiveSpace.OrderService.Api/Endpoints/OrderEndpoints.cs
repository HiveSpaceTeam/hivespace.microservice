using HiveSpace.Core.Exceptions;
using HiveSpace.Core.Exceptions.Models;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.Infrastructure.Authorization;
using HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Commands;
using HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Events;
using HiveSpace.OrderService.Api.Models;
using HiveSpace.OrderService.Application.Interfaces.Messaging;
using HiveSpace.OrderService.Domain.Exceptions;
using HiveSpace.OrderService.Application.Orders.Commands.ConfirmPackage;
using HiveSpace.OrderService.Application.Orders.Commands.RejectPackage;
using HiveSpace.OrderService.Application.Orders.Queries.GetCheckoutStatus;
using HiveSpace.OrderService.Application.Orders.Queries.GetOrderById;
using HiveSpace.OrderService.Application.Orders.Queries.GetOrderList;
using HiveSpace.OrderService.Application.Orders.Queries.GetSellerPackages;
using HiveSpace.OrderService.Application.Cart.Queries.GetCheckoutPreview;
using HiveSpace.Core.Contexts;
using HiveSpace.OrderService.Infrastructure.Data;
using MassTransit;
using MediatR;

namespace HiveSpace.OrderService.Api.Endpoints;

public static class OrderEndpoints
{
    public static IEndpointRouteBuilder MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/orders/checkout", async (
            CheckoutRequest request,
            IBus bus,
            IUserContext userContext,
            CancellationToken ct) =>
        {
            var checkoutClient = bus.CreateRequestClient<CheckoutInitiated>(RequestTimeout.After(m: 2));
            var correlationId = NewId.NextGuid();
            try
            {
                var response = await checkoutClient.GetResponse<CheckoutResponse, CheckoutFailed>(new
                {
                    CorrelationId = correlationId,
                    userContext.UserId,
                    request.DeliveryAddress,
                    CouponCodes = request.CouponCodes ?? []
                }, CancellationToken.None, RequestTimeout.After(m: 2));

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
        .WithTags("Order")
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
        .WithTags("Order")
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
        .WithTags("Order")
        .WithSummary("Get checkout preview")
        .WithDescription("Returns a preview of selected cart items grouped by store, with coupon-applied prices and shipping estimates.");

        app.MapGet("/api/v1/orders", async (
            ISender sender,
            CancellationToken ct,
            int page = 1,
            int pageSize = 20) =>
        {
            var result = await sender.Send(new GetOrderListQuery(page, pageSize), ct);
            return Results.Ok(result);
        })
        .RequireAuthorization()
        .WithName("GetOrderList")
        .WithTags("Order")
        .WithSummary("Get order list")
        .WithDescription("Returns a paginated list of orders for the current user.");

        app.MapGet("/api/v1/orders/{orderId:guid}", async (
            Guid orderId,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new GetOrderByIdQuery(orderId), ct);
            return Results.Ok(result);
        })
        .RequireAuthorization()
        .WithName("GetOrderById")
        .WithTags("Order")
        .WithSummary("Get order by ID")
        .WithDescription("Returns the full order detail including packages for the current user.");

        app.MapGet("/api/v1/orders/seller/packages", async (
            ISender sender,
            CancellationToken ct,
            int page = 1,
            int pageSize = 20,
            string? status = null) =>
        {
            var result = await sender.Send(new GetSellerPackagesQuery(page, pageSize, status), ct);
            return Results.Ok(result);
        })
        .RequireAuthorization(HiveSpaceAuthorizeAttribute.Seller.Policy)
        .WithName("GetSellerPackages")
        .WithTags("Order")
        .WithSummary("Get seller packages")
        .WithDescription("Returns a paginated list of packages belonging to the seller's store.");

        app.MapPost("/api/v1/orders/packages/{packageId:guid}/confirm", async (
            Guid packageId,
            ISender sender,
            IOrderEventPublisher orderEventPublisher,
            IUserContext userContext,
            OrderDbContext db,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new ConfirmPackageCommand(packageId), ct);
            await orderEventPublisher.PublishPackageConfirmedAsync(result, userContext.StoreId!.Value, ct);
            await db.SaveChangesAsync(ct);
            return Results.NoContent();
        })
        .RequireAuthorization(HiveSpaceAuthorizeAttribute.Seller.Policy)
        .WithName("ConfirmPackage")
        .WithTags("Order")
        .WithSummary("Confirm a package")
        .WithDescription("Allows a seller to confirm their package within an order, advancing the checkout saga.");

        app.MapPost("/api/v1/orders/packages/{packageId:guid}/reject", async (
            Guid packageId,
            PackageRejectionRequest request,
            ISender sender,
            IOrderEventPublisher orderEventPublisher,
            OrderDbContext db,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new RejectPackageCommand(packageId, request.Reason), ct);
            await orderEventPublisher.PublishPackageRejectedAsync(result, ct);
            await db.SaveChangesAsync(ct);
            return Results.NoContent();
        })
        .RequireAuthorization(HiveSpaceAuthorizeAttribute.Seller.Policy)
        .WithName("RejectPackage")
        .WithTags("Order")
        .WithSummary("Reject a package")
        .WithDescription("Allows a seller to reject their package within an order with a reason, triggering partial compensation.");

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
