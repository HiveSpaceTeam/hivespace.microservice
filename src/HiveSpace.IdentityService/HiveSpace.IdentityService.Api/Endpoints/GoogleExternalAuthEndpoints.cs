using HiveSpace.IdentityService.Api.Configs;
using HiveSpace.IdentityService.Core.DomainModels;
using HiveSpace.IdentityService.Core.Features.ExternalLogins.Commands.CancelGoogleLink;
using HiveSpace.IdentityService.Core.Features.ExternalLogins.Commands.CompleteGoogleCallback;
using HiveSpace.IdentityService.Core.Features.ExternalLogins.Commands.ConfirmGoogleLink;
using HiveSpace.IdentityService.Core.Features.ExternalLogins.Commands.StartGoogleChallenge;
using HiveSpace.IdentityService.Core.Features.ExternalLogins.Dtos;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace HiveSpace.IdentityService.Api.Endpoints;

internal static class GoogleExternalAuthEndpoints
{
    private const string GoogleCompletionPath = "/api/v1/accounts/external/google/complete";

    public static IEndpointRouteBuilder MapGoogleExternalAuthEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/accounts/external/google/challenge", async (
            [AsParameters] GoogleChallengeQuery query,
            ISender sender,
            SignInManager<ApplicationUser> signInManager,
            IOptions<GoogleExternalAuthOptions> googleOptions,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new StartGoogleChallengeCommand(
                query.App,
                query.ReturnUrl,
                query.Culture), ct);

            if (TryBuildPublicChallengeUrl(httpContext.Request, googleOptions.Value.PublicCallbackOrigin, out var publicChallengeUrl))
                return Results.Redirect(publicChallengeUrl);

            var callbackQuery = QueryString.Create(new Dictionary<string, string?>
            {
                ["app"] = result.App,
                ["returnUrl"] = result.ReturnUrl,
                ["culture"] = result.Culture
            });
            var properties = signInManager.ConfigureExternalAuthenticationProperties(
                GoogleDefaults.AuthenticationScheme,
                GoogleCompletionPath + callbackQuery);

            return Results.Challenge(properties, [GoogleDefaults.AuthenticationScheme]);
        })
        .AllowAnonymous()
        .WithName("StartGoogleExternalLogin")
        .WithTags("Accounts")
        .WithSummary("Start Google sign-in")
        .WithDescription("Starts buyer or seller Google authentication and preserves safe return context.");

        app.MapGet(GoogleCompletionPath, async (
            [AsParameters] GoogleChallengeQuery query,
            ISender sender,
            IConfiguration configuration,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new CompleteGoogleCallbackCommand(
                query.App,
                query.ReturnUrl,
                query.Culture), ct);

            return Results.Redirect(BuildRedirectUrl(configuration, result));
        })
        .AllowAnonymous()
        .WithName("CompleteGoogleExternalLogin")
        .WithTags("Accounts")
        .WithSummary("Complete Google sign-in")
        .WithDescription("Completes Google sign-in, creates a normal user when allowed, or redirects to account linking.");

        app.MapPost("/api/v1/accounts/external/google/link", async (
            ConfirmGoogleLinkRequest request,
            [FromHeader(Name = "X-HiveSpace-CSRF")] string? linkToken,
            ISender sender,
            CancellationToken ct) =>
        {
            var response = await sender.Send(new ConfirmGoogleLinkCommand(
                request.ConsentAccepted,
                request.Password,
                request.App,
                request.ReturnUrl,
                request.Culture,
                linkToken ?? string.Empty), ct);

            return Results.Ok(response);
        })
        .AllowAnonymous()
        .WithName("ConfirmGoogleExternalLoginLink")
        .WithTags("Accounts")
        .WithSummary("Confirm Google account link")
        .WithDescription("Links Google after explicit consent and password confirmation, then establishes a browser session.");

        app.MapDelete("/api/v1/accounts/external/google/link", async (
            [FromHeader(Name = "X-HiveSpace-CSRF")] string? linkToken,
            ISender sender,
            CancellationToken ct) =>
        {
            await sender.Send(new CancelGoogleLinkCommand(linkToken ?? string.Empty), ct);
            return Results.NoContent();
        })
        .AllowAnonymous()
        .WithName("CancelGoogleExternalLoginLink")
        .WithTags("Accounts")
        .WithSummary("Cancel Google account link")
        .WithDescription("Clears pending Google link state without mutating the account or issuing a session.");

        return app;
    }

    private static string BuildRedirectUrl(IConfiguration configuration, GoogleCallbackResult result)
    {
        return result.Outcome switch
        {
            GoogleCallbackOutcome.PendingLink => BuildFrontendUrl(configuration, result.App, "/auth/google/link", new Dictionary<string, string?>
            {
                ["linkToken"] = result.LinkToken,
                ["returnUrl"] = result.ReturnUrl,
                ["culture"] = result.Culture
            }),
            GoogleCallbackOutcome.Failed => BuildFrontendUrl(configuration, result.App, GetLoginPath(configuration, result.App), new Dictionary<string, string?>
            {
                ["error"] = result.ErrorCode,
                ["returnUrl"] = result.ReturnUrl,
                ["culture"] = result.Culture
            }),
            _ => BuildFrontendUrl(configuration, result.App, result.ReturnUrl ?? "/", new Dictionary<string, string?>
            {
                ["culture"] = result.Culture
            })
        };
    }

    private static string BuildFrontendUrl(
        IConfiguration configuration,
        string app,
        string path,
        IDictionary<string, string?> query)
    {
        var origin = configuration[$"FrontendRedirects:{app}:Origin"]
            ?? configuration["DefaultRedirectUrl"]
            ?? "http://localhost:5175";

        var builder = new UriBuilder(new Uri(new Uri(origin.TrimEnd('/')), path.TrimStart('/')));
        var queryString = QueryString.Create(query.Where(kvp => !string.IsNullOrWhiteSpace(kvp.Value)));
        builder.Query = queryString.Value?.TrimStart('?') ?? string.Empty;
        return builder.Uri.ToString();
    }

    private static string GetLoginPath(IConfiguration configuration, string app)
        => configuration[$"FrontendRedirects:{app}:loginPath"] ?? "/signin";

    private static bool TryBuildPublicChallengeUrl(HttpRequest request, string? publicCallbackOrigin, out string redirectUrl)
    {
        redirectUrl = string.Empty;

        if (string.IsNullOrWhiteSpace(publicCallbackOrigin)
            || !Uri.TryCreate(publicCallbackOrigin.TrimEnd('/'), UriKind.Absolute, out var publicOrigin))
        {
            return false;
        }

        var requestOrigin = new UriBuilder(request.Scheme, request.Host.Host, request.Host.Port ?? -1).Uri;
        if (SameOrigin(requestOrigin, publicOrigin))
            return false;

        var publicBase = new Uri(publicOrigin.GetLeftPart(UriPartial.Authority).TrimEnd('/') + "/");
        var publicPath = $"{request.PathBase}{request.Path}".TrimStart('/');
        var builder = new UriBuilder(new Uri(publicBase, publicPath))
        {
            Query = request.QueryString.Value?.TrimStart('?') ?? string.Empty
        };

        redirectUrl = builder.Uri.ToString();
        return true;
    }

    private static bool SameOrigin(Uri left, Uri right)
        => string.Equals(left.Scheme, right.Scheme, StringComparison.OrdinalIgnoreCase)
            && string.Equals(left.Host, right.Host, StringComparison.OrdinalIgnoreCase)
            && left.Port == right.Port;

    private sealed record GoogleChallengeQuery(
        [property: FromQuery(Name = "app")] string App,
        [property: FromQuery(Name = "returnUrl")] string? ReturnUrl,
        [property: FromQuery(Name = "culture")] string? Culture);
}
