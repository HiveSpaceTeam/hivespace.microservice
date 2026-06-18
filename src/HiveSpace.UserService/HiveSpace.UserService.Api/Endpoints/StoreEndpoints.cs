using HiveSpace.Infrastructure.Authorization;
using HiveSpace.UserService.Application.Stores.Commands.CreateStore;
using HiveSpace.UserService.Application.Stores.Dtos;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace HiveSpace.UserService.Api.Endpoints;

public static class StoreEndpoints
{
    public static IEndpointRouteBuilder MapStoreEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/stores")
            .WithTags("Stores");

        group.MapPost("/", async (
            [FromBody] CreateStoreRequestDto request,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(new CreateStoreCommand(request), ct);
            return Results.Created($"/api/v1/stores/{result.StoreId}", result);
        })
        .RequireAuthorization(HiveSpaceAuthorizeAttribute.User.Policy)
        .WithName("CreateStore")
        .Produces<CreateStoreResponseDto>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status409Conflict);

        return app;
    }
}
