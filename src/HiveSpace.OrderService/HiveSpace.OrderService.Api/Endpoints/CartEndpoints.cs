using HiveSpace.OrderService.Application.Cart.Commands.AddCartItem;
using HiveSpace.OrderService.Application.Cart.Commands.ApplyPlatformCoupon;
using HiveSpace.OrderService.Application.Cart.Commands.ApplyStoreCoupon;
using HiveSpace.OrderService.Application.Cart.Commands.RemoveCartItem;
using HiveSpace.OrderService.Application.Cart.Commands.RemovePlatformCoupon;
using HiveSpace.OrderService.Application.Cart.Commands.RemoveStoreCoupon;
using HiveSpace.OrderService.Application.Cart.Commands.UpdateCartItems;
using FluentValidation;
using HiveSpace.Core.Helpers;
using HiveSpace.OrderService.Api.Models;
using HiveSpace.OrderService.Application.Cart.Queries.GetCartSummary;
using HiveSpace.OrderService.Application.Cart.Queries.GetSelectedCartItemsCount;
using HiveSpace.Infrastructure.Authorization;
using MediatR;

namespace HiveSpace.OrderService.Api.Endpoints;

public static class CartEndpoints
{
    public static void MapCartEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/carts/summary", async (
            GetCartSummaryRequest? request,
            IValidator<GetCartSummaryQuery> validator,
            ISender sender,
            CancellationToken ct) =>
        {
            request ??= new GetCartSummaryRequest();

            var query = new GetCartSummaryQuery(
                request.Page,
                request.PageSize);

            var result = await sender.Send(query, ct);
            return Results.Ok(result);
        })
        .RequireAuthorization(HiveSpaceAuthorizeAttribute.User.Policy)
        .WithTags("Cart")
        .WithOpenApi()
        .Produces<GetCartSummaryResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithSummary("Get cart summary with coupon calculations");

        app.MapPost("/api/v1/carts/coupons/platform", async (
            ApplyCouponRequest request,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new ApplyPlatformCouponCommand(request.CouponCode), ct);
            return Results.Ok(result);
        })
        .RequireAuthorization(HiveSpaceAuthorizeAttribute.User.Policy)
        .WithTags("Cart")
        .WithOpenApi()
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithSummary("Apply a platform coupon to the cart");

        app.MapDelete("/api/v1/carts/coupons/platform/{couponCode}", async (
            string couponCode,
            ISender sender,
            CancellationToken ct) =>
        {
            await sender.Send(new RemovePlatformCouponCommand(couponCode), ct);
            return Results.NoContent();
        })
        .RequireAuthorization(HiveSpaceAuthorizeAttribute.User.Policy)
        .WithTags("Cart")
        .WithOpenApi()
        .Produces(StatusCodes.Status204NoContent)
        .WithSummary("Remove a platform coupon from the cart");

        app.MapPut("/api/v1/carts/coupons/stores/{storeId:guid}", async (
            Guid storeId,
            ApplyCouponRequest request,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new ApplyStoreCouponCommand(storeId, request.CouponCode), ct);
            return Results.Ok(result);
        })
        .RequireAuthorization(HiveSpaceAuthorizeAttribute.User.Policy)
        .WithTags("Cart")
        .WithOpenApi()
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithSummary("Apply a store coupon to the cart");

        app.MapDelete("/api/v1/carts/coupons/stores/{storeId:guid}", async (
            Guid storeId,
            ISender sender,
            CancellationToken ct) =>
        {
            await sender.Send(new RemoveStoreCouponCommand(storeId), ct);
            return Results.NoContent();
        })
        .RequireAuthorization(HiveSpaceAuthorizeAttribute.User.Policy)
        .WithTags("Cart")
        .WithOpenApi()
        .Produces(StatusCodes.Status204NoContent)
        .WithSummary("Remove a store coupon from the cart");

        var group = app.MapGroup("/api/v1/carts/items")
            .WithTags("Cart")
            .WithOpenApi()
            .RequireAuthorization(HiveSpaceAuthorizeAttribute.User.Policy);

        group.MapPost("/", async (AddCartItemCommand command, ISender sender) =>
        {
            var cartItemId = await sender.Send(command);
            return Results.Created($"/api/v1/carts/items/{cartItemId}", new { cartItemId });
        })
        .Produces(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithSummary("Add item to cart");

        group.MapDelete("/{cartItemId:guid}", async (Guid cartItemId, ISender sender) =>
        {
            await sender.Send(new RemoveCartItemCommand(cartItemId));
            return Results.NoContent();
        })
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithSummary("Remove item from cart");

        group.MapPut("/", async (UpdateCartItemsCommand command, ISender sender) =>
        {
            await sender.Send(command);
            return Results.NoContent();
        })
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithSummary("Update cart items (quantity, selection, or select all)");

        group.MapGet("/selected/count", async (ISender sender, CancellationToken ct) =>
        {
            var count = await sender.Send(new GetSelectedCartItemsCountQuery(), ct);
            return Results.Ok(new { count });
        })
        .Produces(StatusCodes.Status200OK)
        .WithSummary("Get selected cart item count");
    }
}
