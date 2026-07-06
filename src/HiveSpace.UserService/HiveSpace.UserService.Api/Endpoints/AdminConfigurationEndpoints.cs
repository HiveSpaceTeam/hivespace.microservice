using HiveSpace.Infrastructure.Authorization;
using HiveSpace.UserService.Application.Configuration.Commands.UpdatePlatformCurrencyConfig;
using HiveSpace.UserService.Application.Configuration.Dtos;
using HiveSpace.UserService.Application.Configuration.Queries.GetPlatformCurrencyConfig;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace HiveSpace.UserService.Api.Endpoints;

public static class AdminConfigurationEndpoints
{
    public static IEndpointRouteBuilder MapAdminConfigurationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/admins/configuration")
            .WithTags("AdminConfiguration")
            .RequireAuthorization(HiveSpaceAuthorizeAttribute.Admin.Policy);

        group.MapGet("/currencies", async (IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetPlatformCurrencyConfigQuery(), ct);
            return Results.Ok(result);
        })
        .WithName("GetPlatformCurrencyConfig")
        .Produces<PlatformCurrencyConfigDto>()
        .Produces(StatusCodes.Status404NotFound);

        group.MapPut("/currencies", async (
            [FromBody] UpdatePlatformCurrencyConfigCommand command,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(command, ct);
            return Results.Ok(result);
        })
        .WithName("UpdatePlatformCurrencyConfig")
        .Produces<PlatformCurrencyConfigDto>()
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status409Conflict);

        return app;
    }
}
