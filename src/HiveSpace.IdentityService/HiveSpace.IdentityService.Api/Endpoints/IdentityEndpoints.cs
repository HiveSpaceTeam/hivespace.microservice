using HiveSpace.Core.Contexts;
using HiveSpace.IdentityService.Core.Features.AccountSessions.Commands.RefreshSession;
using HiveSpace.IdentityService.Core.Features.AccountSessions.Commands.RegisterAccount;
using HiveSpace.IdentityService.Core.Features.AccountSessions.Commands.SignIn;
using HiveSpace.IdentityService.Core.Features.AccountSessions.Commands.SignOut;
using HiveSpace.IdentityService.Core.Features.AccountSessions.Dtos;
using HiveSpace.IdentityService.Core.Features.EmailVerification.Commands.ConfirmEmailVerification;
using HiveSpace.IdentityService.Core.Features.EmailVerification.Commands.SendEmailVerification;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace HiveSpace.IdentityService.Api.Endpoints;

internal static class IdentityEndpoints
{
    public static IEndpointRouteBuilder MapIdentityEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/accounts/login", async (
            SignInRequest request,
            ISender sender,
            CancellationToken ct) =>
        {
            var response = await sender.Send(new SignInCommand(
                request.Email,
                request.Password,
                request.App,
                request.ReturnUrl,
                request.Culture), ct);

            return Results.Ok(response);
        })
        .AllowAnonymous()
        .WithName("SignIn")
        .WithTags("Accounts")
        .WithSummary("Sign in with email and password")
        .WithDescription("Validates credentials and establishes a secure HttpOnly browser session.");

        app.MapPost("/api/v1/accounts/register", async (
            RegisterAccountRequest request,
            ISender sender,
            CancellationToken ct) =>
        {
            var response = await sender.Send(new RegisterAccountCommand(
                request.Email,
                request.Password,
                request.ConfirmPassword,
                request.FullName,
                request.App,
                request.ReturnUrl,
                request.Culture), ct);

            return Results.Json(response, statusCode: StatusCodes.Status201Created);
        })
        .AllowAnonymous()
        .WithName("RegisterAccount")
        .WithTags("Accounts")
        .WithSummary("Register a browser account")
        .WithDescription("Creates an identity account, preserves profile creation events, and establishes a browser session.");

        app.MapPost("/api/v1/accounts/session/refresh", async (
            RefreshSessionRequest request,
            ISender sender,
            CancellationToken ct) =>
        {
            var response = await sender.Send(new RefreshSessionCommand(request.App), ct);
            return Results.Ok(response);
        })
        .AddEndpointFilter<RequireBrowserSessionFilter>()
        .WithName("RefreshAccountSession")
        .WithTags("Accounts")
        .WithSummary("Refresh browser session")
        .WithDescription("Refreshes the protected browser session and rotates the CSRF token.");

        app.MapPost("/api/v1/accounts/logout", async (
            ISender sender,
            CancellationToken ct) =>
        {
            await sender.Send(new SignOutCommand(), ct);
            return Results.NoContent();
        })
        .WithName("SignOut")
        .WithTags("Accounts")
        .WithSummary("Sign out")
        .WithDescription("Clears the browser session and CSRF cookies.");

        app.MapPost("/api/v1/accounts/email-verification", async (
            SendEmailVerificationRequest request,
            [FromServices] IUserContext userContext,
            ISender sender,
            CancellationToken ct) =>
        {
            await sender.Send(new SendEmailVerificationCommand(userContext.UserId, request.CallbackUrl, request.ReturnUrl), ct);
            return Results.Accepted();
        })
        .RequireAuthorization("RequireAdminOrUser")
        .WithName("SendEmailVerification")
        .WithTags("Accounts")
        .WithSummary("Send email verification")
        .WithDescription("Requests an identity-owned email verification message for the current user.");

        app.MapPost("/api/v1/accounts/email-verification/verify", async (
            ConfirmEmailVerificationRequest request,
            ISender sender,
            CancellationToken ct) =>
        {
            await sender.Send(new ConfirmEmailVerificationCommand(request.UserId, request.Token), ct);
            return Results.NoContent();
        })
        .AllowAnonymous()
        .WithName("ConfirmEmailVerification")
        .WithTags("Accounts")
        .WithSummary("Confirm email verification")
        .WithDescription("Confirms an identity-owned email verification token.");

        return app;
    }
}

internal record SendEmailVerificationRequest(string CallbackUrl, string? ReturnUrl);

internal record ConfirmEmailVerificationRequest(string UserId, string Token);
