using HiveSpace.OrderService.Application.Coupons.Commands.CreateCoupon;
using HiveSpace.OrderService.Application.Coupons.Commands.UpdateCoupon;
using HiveSpace.OrderService.Application.Coupons.Commands.DeleteCoupon;
using HiveSpace.OrderService.Application.Coupons.Commands.EndCoupon;
using HiveSpace.OrderService.Application.Coupons.Dtos;
using HiveSpace.OrderService.Application.Coupons.Queries.GetCouponById;
using HiveSpace.OrderService.Application.Coupons.Queries.GetCouponList;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.OrderService.Domain.Exceptions;

namespace HiveSpace.OrderService.Api.Endpoints;

public static class CouponEndpoints
{
    public static void MapCouponEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/coupons")
            .WithTags("Coupons")
            .WithOpenApi();

        group.MapPost("/", async (CreateCouponCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return Results.Created($"/api/v1/coupons/{result.Id}", result);
        })
        .Produces<CouponDto>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithSummary("Create a new coupon")
        .RequireAuthorization();

        group.MapGet("/", async ([AsParameters] GetCouponListQuery query, ISender sender) =>
        {
            var result = await sender.Send(query);
            return Results.Ok(result);
        })
        .Produces<GetCouponListResponse>(StatusCodes.Status200OK)
        .WithSummary("Get paginated list of coupons")
        .RequireAuthorization();

        group.MapGet("/{id:guid}", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new GetCouponByIdQuery(id));
            return Results.Ok(result);
        })
        .Produces<CouponDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithSummary("Get a coupon by ID")
        .RequireAuthorization();

        group.MapDelete("/{id:guid}", async (Guid id, ISender sender) =>
        {
            await sender.Send(new DeleteCouponCommand(id));
            return Results.NoContent();
        })
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .WithSummary("Delete a coupon by ID")
        .RequireAuthorization();

        group.MapPost("/{id:guid}/end", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new EndCouponCommand(id));
            return Results.Ok(result);
        })
        .Produces<CouponDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithSummary("End an upcoming coupon")
        .RequireAuthorization();

        group.MapPut("/{id:guid}", async (Guid id, UpdateCouponCommand command, ISender sender) =>
        {
            if (id != command.Id)
            {
                throw new InvalidFieldException(OrderDomainErrorCode.CouponInvalid, nameof(command.Id));
            }
            var result = await sender.Send(command);
            return Results.Ok(result);
        })
        .Produces<CouponDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .WithSummary("Update a coupon by ID")
        .RequireAuthorization();
    }
}
