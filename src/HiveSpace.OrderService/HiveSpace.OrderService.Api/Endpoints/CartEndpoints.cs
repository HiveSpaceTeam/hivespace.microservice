using HiveSpace.OrderService.Application.Cart.Commands.AddCartItem;
using HiveSpace.OrderService.Application.Cart.Commands.RemoveCartItem;
using HiveSpace.OrderService.Application.Cart.Commands.UpdateCartItems;
using HiveSpace.OrderService.Application.Cart.Queries.GetCartItems;
using HiveSpace.Infrastructure.Authorization;
using MediatR;

namespace HiveSpace.OrderService.Api.Endpoints;

public static class CartEndpoints
{
    public static void MapCartEndpoints(this IEndpointRouteBuilder app)
    {
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

        group.MapGet("/", async ([AsParameters] GetCartItemsQuery query, ISender sender) =>
        {
            var result = await sender.Send(query);
            return Results.Ok(result);
        })
        .Produces<GetCartItemsResponse>(StatusCodes.Status200OK)
        .WithSummary("Get paginated cart items");
    }
}
