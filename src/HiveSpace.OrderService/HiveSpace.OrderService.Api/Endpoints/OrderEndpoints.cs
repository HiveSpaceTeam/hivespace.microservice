using HiveSpace.Infrastructure.Authorization;
using HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Commands;
using HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Events;
using HiveSpace.OrderService.Api.Models;
using HiveSpace.OrderService.Application.Orders.Commands.ConfirmPackage;
using HiveSpace.OrderService.Application.Orders.Commands.RejectPackage;
using HiveSpace.OrderService.Application.Orders.Queries.GetCheckoutStatus;
using HiveSpace.OrderService.Application.Orders.Queries.GetOrderById;
using HiveSpace.OrderService.Application.Orders.Queries.GetOrderList;
using HiveSpace.OrderService.Application.Orders.Queries.GetSellerPackages;
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
            IPublishEndpoint bus,
            IUserContext userContext,
            OrderDbContext db,
            CancellationToken ct) =>
        {
            var correlationId = NewId.NextGuid();
            await bus.Publish<CheckoutInitiated>(new
            {
                CorrelationId = correlationId,
                userContext.UserId,
                request.DeliveryAddress,
                CouponCodes = request.CouponCodes ?? []
            }, ct);
            await db.SaveChangesAsync(ct);
            return Results.NoContent();
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
            return result is null ? Results.NotFound() : Results.Ok(result);
        })
        .RequireAuthorization()
        .WithName("GetCheckoutStatus")
        .WithTags("Order")
        .WithSummary("Get checkout status")
        .WithDescription("Returns the checkout saga status by orderId. The orderId is the correlationId returned when checkout is initiated.");

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
            return result is null ? Results.NotFound() : Results.Ok(result);
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
            IPublishEndpoint bus,
            IUserContext userContext,
            OrderDbContext db,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new ConfirmPackageCommand(packageId), ct);

            await bus.Publish<PackageConfirmed>(new
            {
                CorrelationId = result.OrderId,
                OrderId       = result.OrderId,
                PackageId     = result.PackageId,
                StoreId       = userContext.StoreId ?? Guid.Empty,
                ConfirmedAt   = DateTimeOffset.UtcNow
            }, ct);
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
            IPublishEndpoint bus,
            OrderDbContext db,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new RejectPackageCommand(packageId, request.Reason), ct);

            await bus.Publish<PackageRejected>(new
            {
                CorrelationId = result.OrderId,
                OrderId       = result.OrderId,
                PackageId     = result.PackageId,
                result.Reason,
                PackageAmount = result.PackageAmount
            }, ct);
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
}
