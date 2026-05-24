using HiveSpace.Core.Contexts;
using HiveSpace.IdentityService.Core.Features.AdminIdentity.Commands.CreateAdmin;
using HiveSpace.IdentityService.Core.Features.AdminIdentity.Commands.DeleteUser;
using HiveSpace.IdentityService.Core.Features.AdminIdentity.Commands.SetUserStatus;
using HiveSpace.IdentityService.Core.Features.AdminIdentity.Queries.GetAdmins;
using HiveSpace.IdentityService.Core.Features.AdminIdentity.Queries.GetUsers;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace HiveSpace.IdentityService.Api.Endpoints;

internal static class AdminIdentityEndpoints
{
    public static IEndpointRouteBuilder MapAdminIdentityEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/admins", async (
            CreateAdminRequest request,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new CreateAdminCommand(
                request.Email,
                request.Password,
                request.FullName,
                request.ConfirmPassword,
                request.IsSystemAdmin), ct);

            return Results.Created($"/api/v1/admins/{result.Id}", result);
        })
        .RequireAuthorization("RequireAdmin")
        .WithName("CreateAdmin")
        .WithTags("Admin Identity")
        .WithSummary("Create admin account")
        .WithDescription("Creates an identity-owned admin or system admin account.");

        app.MapGet("/api/v1/admins", async (
            ISender sender,
            CancellationToken ct,
            int page = 1,
            int pageSize = 20,
            string? searchTerm = null) =>
        {
            var result = await sender.Send(new GetAdminsQuery(page, pageSize, searchTerm), ct);
            return Results.Ok(result);
        })
        .RequireAuthorization("RequireAdmin")
        .WithName("GetAdmins")
        .WithTags("Admin Identity")
        .WithSummary("Get admin accounts")
        .WithDescription("Returns a paginated list of identity-owned admin accounts.");

        app.MapGet("/api/v1/admins/users", async (
            ISender sender,
            CancellationToken ct,
            int page = 1,
            int pageSize = 20,
            string? searchTerm = null) =>
        {
            var result = await sender.Send(new GetUsersQuery(page, pageSize, searchTerm), ct);
            return Results.Ok(result);
        })
        .RequireAuthorization("RequireAdmin")
        .WithName("GetIdentityUsers")
        .WithTags("Admin Identity")
        .WithSummary("Get identity users")
        .WithDescription("Returns a paginated list of identity-owned buyer and seller accounts.");

        app.MapPut("/api/v1/admins/users/status", async (
            SetUserStatusRequest request,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new SetUserStatusCommand(request.UserId, request.IsActive), ct);
            return Results.Ok(result);
        })
        .RequireAuthorization("RequireAdmin")
        .WithName("SetIdentityUserStatus")
        .WithTags("Admin Identity")
        .WithSummary("Set identity user status")
        .WithDescription("Activates or suspends identity-owned account status.");

        app.MapDelete("/api/v1/admins/users/{userId:guid}", async (
            Guid userId,
            [FromServices] IUserContext userContext,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new DeleteUserCommand(userId, userContext.Email), ct);
            return Results.Ok(result);
        })
        .RequireAuthorization("RequireAdmin")
        .WithName("DeleteIdentityUser")
        .WithTags("Admin Identity")
        .WithSummary("Delete identity user")
        .WithDescription("Deactivates an identity-owned user account.");

        return app;
    }
}

internal record CreateAdminRequest(
    string Email,
    string Password,
    string FullName,
    string ConfirmPassword,
    bool IsSystemAdmin = false);

internal record SetUserStatusRequest(Guid UserId, bool IsActive, int ResponseType);
