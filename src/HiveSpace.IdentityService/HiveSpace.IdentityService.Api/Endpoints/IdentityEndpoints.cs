using HiveSpace.Core.Contexts;
using HiveSpace.IdentityService.Core.Features.EmailVerification.Commands.ConfirmEmailVerification;
using HiveSpace.IdentityService.Core.Features.EmailVerification.Commands.SendEmailVerification;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace HiveSpace.IdentityService.Api.Endpoints;

internal static class IdentityEndpoints
{
    public static IEndpointRouteBuilder MapIdentityEndpoints(this IEndpointRouteBuilder app)
    {
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
