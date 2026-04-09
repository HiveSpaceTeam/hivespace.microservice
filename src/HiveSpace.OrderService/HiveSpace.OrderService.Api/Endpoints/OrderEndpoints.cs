using HiveSpace.Infrastructure.Authorization;
using HiveSpace.OrderService.Api.Models;
using HiveSpace.OrderService.Application.Interfaces.Messaging;
using HiveSpace.OrderService.Application.Orders.Commands.ConfirmOrder;
using HiveSpace.OrderService.Application.Orders.Commands.RejectOrder;
using HiveSpace.OrderService.Application.Orders.Enums;
using HiveSpace.OrderService.Application.Orders.Queries.GetOrderById;
using HiveSpace.OrderService.Application.Orders.Queries.GetOrderList;
using HiveSpace.OrderService.Application.Orders.Queries.GetSellerOrders;
using HiveSpace.OrderService.Infrastructure.Data;
using MediatR;

namespace HiveSpace.OrderService.Api.Endpoints;

public static class OrderEndpoints
{
    public static IEndpointRouteBuilder MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/orders", async (
            ISender sender,
            CancellationToken ct,
            int page = 1,
            int pageSize = 20,
            CustomerOrderProcessStatus? processStatus = null,
            string? searchField = null,
            string? searchValue = null) =>
        {
            var result = await sender.Send(
                new GetOrderListQuery(page, pageSize, processStatus, searchField, searchValue), ct);
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
        .WithDescription("Returns the full order detail for the current user.");

        app.MapGet("/api/v1/orders/seller", async (
            ISender sender,
            CancellationToken ct,
            int page = 1,
            int pageSize = 20,
            SellerOrderProcessStatus? processStatus = null,
            string? searchField = null,
            string? searchValue = null) =>
        {
            var result = await sender.Send(
                new GetSellerOrdersQuery(page, pageSize, processStatus, searchField, searchValue), ct);
            return Results.Ok(result);
        })
        .RequireAuthorization(HiveSpaceAuthorizeAttribute.Seller.Policy)
        .WithName("GetSellerOrders")
        .WithTags("Order")
        .WithSummary("Get seller orders")
        .WithDescription("Returns a paginated list of orders belonging to the seller's store.");

        app.MapPost("/api/v1/orders/{orderId:guid}/confirm", async (
            Guid orderId,
            ISender sender,
            IOrderEventPublisher orderEventPublisher,
            OrderDbContext db,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new ConfirmOrderCommand(orderId), ct);
            await orderEventPublisher.PublishOrderConfirmedBySellerAsync(result, ct);
            await db.SaveChangesAsync(ct);
            return Results.NoContent();
        })
        .RequireAuthorization(HiveSpaceAuthorizeAttribute.Seller.Policy)
        .WithName("ConfirmOrder")
        .WithTags("Order")
        .WithSummary("Confirm an order")
        .WithDescription("Allows a seller to confirm their order, advancing the fulfillment saga.");

        app.MapPost("/api/v1/orders/{orderId:guid}/reject", async (
            Guid orderId,
            PackageRejectionRequest request,
            ISender sender,
            IOrderEventPublisher orderEventPublisher,
            OrderDbContext db,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new RejectOrderCommand(orderId, request.Reason), ct);
            await orderEventPublisher.PublishOrderRejectedBySellerAsync(result, ct);
            await db.SaveChangesAsync(ct);
            return Results.NoContent();
        })
        .RequireAuthorization(HiveSpaceAuthorizeAttribute.Seller.Policy)
        .WithName("RejectOrder")
        .WithTags("Order")
        .WithSummary("Reject an order")
        .WithDescription("Allows a seller to reject their order with a reason, triggering compensation.");

        return app;
    }
}
