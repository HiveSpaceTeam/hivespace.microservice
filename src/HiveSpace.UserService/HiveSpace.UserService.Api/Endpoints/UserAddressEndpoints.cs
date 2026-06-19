using HiveSpace.Infrastructure.Authorization;
using HiveSpace.UserService.Application.UserAddresses.Commands.CreateUserAddress;
using HiveSpace.UserService.Application.UserAddresses.Commands.DeleteUserAddress;
using HiveSpace.UserService.Application.UserAddresses.Commands.SetDefaultUserAddress;
using HiveSpace.UserService.Application.UserAddresses.Commands.UpdateUserAddress;
using HiveSpace.UserService.Application.UserAddresses.Dtos;
using HiveSpace.UserService.Application.UserAddresses.Queries.GetDefaultUserAddress;
using HiveSpace.UserService.Application.UserAddresses.Queries.GetUserAddressById;
using HiveSpace.UserService.Application.UserAddresses.Queries.GetUserAddresses;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace HiveSpace.UserService.Api.Endpoints;

public static class UserAddressEndpoints
{
    public static IEndpointRouteBuilder MapUserAddressEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/users/address")
            .WithTags("UserAddresses")
            .RequireAuthorization(HiveSpaceAuthorizeAttribute.AdminOrUser.Policy);

        group.MapGet("/", async (IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetUserAddressesQuery(), ct);
            return Results.Ok(result);
        })
        .WithName("GetUserAddresses")
        .Produces<List<UserAddressDto>>();

        group.MapGet("/default", async (IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetDefaultUserAddressQuery(), ct);
            return result is null ? Results.NotFound() : Results.Ok(result);
        })
        .WithName("GetDefaultUserAddress")
        .Produces<UserAddressDto>()
        .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/{addressId:guid}", async (
            Guid addressId,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetUserAddressByIdQuery(addressId), ct);
            return Results.Ok(result);
        })
        .WithName("GetUserAddressById")
        .Produces<UserAddressDto?>();

        group.MapPost("/", async (
            [FromBody] UserAddressRequestDto request,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(new CreateUserAddressCommand(request), ct);
            return Results.Created($"/api/v1/users/address/{result.Id}", result);
        })
        .WithName("CreateUserAddress")
        .Produces<UserAddressDto>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest);

        group.MapPut("/{addressId:guid}", async (
            Guid addressId,
            [FromBody] UserAddressRequestDto request,
            IMediator mediator,
            CancellationToken ct) =>
        {
            await mediator.Send(new UpdateUserAddressCommand(addressId, request), ct);
            return Results.NoContent();
        })
        .WithName("UpdateUserAddress")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest);

        group.MapPut("/{addressId:guid}/default", async (
            Guid addressId,
            IMediator mediator,
            CancellationToken ct) =>
        {
            await mediator.Send(new SetDefaultUserAddressCommand(addressId), ct);
            return Results.NoContent();
        })
        .WithName("SetDefaultUserAddress")
        .Produces(StatusCodes.Status204NoContent);

        group.MapDelete("/{addressId:guid}", async (
            Guid addressId,
            IMediator mediator,
            CancellationToken ct) =>
        {
            await mediator.Send(new DeleteUserAddressCommand(addressId), ct);
            return Results.NoContent();
        })
        .WithName("DeleteUserAddress")
        .Produces(StatusCodes.Status204NoContent);

        return app;
    }
}
