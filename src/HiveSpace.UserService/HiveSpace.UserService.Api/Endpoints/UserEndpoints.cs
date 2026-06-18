using HiveSpace.Infrastructure.Authorization;
using HiveSpace.UserService.Application.Users.Commands.UpdateUserProfile;
using HiveSpace.UserService.Application.Users.Commands.UpdateUserSetting;
using HiveSpace.UserService.Application.Users.Dtos;
using HiveSpace.UserService.Application.Users.Queries.GetUserProfile;
using HiveSpace.UserService.Application.Users.Queries.GetUserSetting;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace HiveSpace.UserService.Api.Endpoints;

public static class UserEndpoints
{
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/users")
            .WithTags("Users")
            .RequireAuthorization(HiveSpaceAuthorizeAttribute.AdminOrUser.Policy);

        group.MapGet("/me", async (IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetUserProfileQuery(), ct);
            return Results.Ok(result);
        })
        .WithName("GetUserProfile")
        .Produces<GetUserProfileResponseDto>()
        .Produces(StatusCodes.Status404NotFound);

        group.MapPut("/me", async (
            [FromBody] UpdateUserProfileRequestDto request,
            IMediator mediator,
            CancellationToken ct) =>
        {
            await mediator.Send(new UpdateUserProfileCommand(request), ct);
            return Results.NoContent();
        })
        .WithName("UpdateUserProfile")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status409Conflict);

        group.MapGet("/settings", async (IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetUserSettingQuery(), ct);
            return Results.Ok(result);
        })
        .WithName("GetUserSetting")
        .Produces<GetUserSettingsResponseDto>()
        .Produces(StatusCodes.Status404NotFound);

        group.MapPut("/settings", async (
            [FromBody] UpdateUserSettingRequestDto request,
            IMediator mediator,
            CancellationToken ct) =>
        {
            await mediator.Send(new UpdateUserSettingCommand(request), ct);
            return Results.NoContent();
        })
        .WithName("UpdateUserSetting")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);

        return app;
    }
}
