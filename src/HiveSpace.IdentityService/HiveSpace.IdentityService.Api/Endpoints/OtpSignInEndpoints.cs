using HiveSpace.IdentityService.Api.Contracts;
using HiveSpace.IdentityService.Core.Exceptions;
using HiveSpace.IdentityService.Core.Features.AccountSessions.Commands.RequestOtpSignIn;
using HiveSpace.IdentityService.Core.Features.AccountSessions.Commands.VerifyOtpSignIn;
using HiveSpace.Core.Exceptions;
using MediatR;

namespace HiveSpace.IdentityService.Api.Endpoints;

internal static class OtpSignInEndpoints
{
    public static IEndpointRouteBuilder MapOtpSignInEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/accounts/otp/request", async (
            RequestOtpSignInRequest request,
            ISender sender,
            CancellationToken ct) =>
        {
            var response = await sender.Send(new RequestOtpSignInCommand(request.Email), ct);
            return Results.Ok(response);
        })
        .AllowAnonymous()
        .WithName("RequestOtpSignIn")
        .WithTags("Accounts")
        .WithSummary("Request an OTP sign-in code")
        .WithDescription("Returns a generic success response and sends an OTP email only for eligible buyer or seller accounts.");

        app.MapPost("/api/v1/accounts/otp/verify", async (
            VerifyOtpSignInRequest request,
            HttpContext httpContext,
            IConfiguration configuration,
            ISender sender,
            CancellationToken ct) =>
        {
            try
            {
                var appName = ResolveApp(httpContext.Request, configuration);
                var returnUrl = ResolveReturnUrl(httpContext.Request, configuration, appName);
                var response = await sender.Send(new VerifyOtpSignInCommand(
                    request.ChallengeToken,
                    request.Code,
                    appName,
                    returnUrl), ct);

                return Results.Ok(response);
            }
            catch (UnauthorizedException ex) when (TryMapOtpVerifyError(ex, out var error))
            {
                return Results.Json(new { error }, statusCode: StatusCodes.Status401Unauthorized);
            }
        })
        .AllowAnonymous()
        .WithName("VerifyOtpSignIn")
        .WithTags("Accounts")
        .WithSummary("Verify an OTP sign-in code")
        .WithDescription("Verifies an OTP challenge token and code, then establishes the same browser session as password sign-in.");

        return app;
    }

    private static string ResolveApp(HttpRequest request, IConfiguration configuration)
    {
        var appHeader = request.Headers["X-HiveSpace-App"].ToString();
        if (IsKnownApp(appHeader))
            return NormalizeApp(appHeader);

        var originCandidates = new[]
        {
            request.Headers["Origin"].ToString(),
            request.Headers["Referer"].ToString()
        };

        foreach (var candidate in originCandidates)
        {
            if (!Uri.TryCreate(candidate, UriKind.Absolute, out var candidateUri))
                continue;

            foreach (var app in new[] { "seller", "buyer", "admin" })
            {
                var configuredOrigin = configuration[$"FrontendRedirects:{app}:Origin"];
                if (!Uri.TryCreate(configuredOrigin, UriKind.Absolute, out var configuredUri))
                    continue;

                if (SameOrigin(candidateUri, configuredUri))
                    return app;
            }
        }

        return "buyer";
    }

    private static string? ResolveReturnUrl(HttpRequest request, IConfiguration configuration, string app)
    {
        var candidate = request.Headers["X-HiveSpace-ReturnUrl"].ToString();
        if (string.IsNullOrWhiteSpace(candidate))
            return null;

        if (Uri.TryCreate(candidate, UriKind.Relative, out _))
        {
            return IsSafeReturnUrl(candidate) ? candidate : null;
        }

        if (!Uri.TryCreate(candidate, UriKind.Absolute, out var absoluteCandidate))
            return null;

        var allowedOrigin = configuration[$"FrontendRedirects:{app}:Origin"];
        if (!Uri.TryCreate(allowedOrigin, UriKind.Absolute, out var allowedUri) || !SameOrigin(absoluteCandidate, allowedUri))
            return null;

        var relativePath = absoluteCandidate.PathAndQuery;
        return IsSafeReturnUrl(relativePath) ? relativePath : null;
    }

    private static bool TryMapOtpVerifyError(UnauthorizedException exception, out string error)
    {
        var errorName = exception.ErrorCodeList.FirstOrDefault()?.ErrorCode.Name;
        error = errorName switch
        {
            nameof(IdentityDomainErrorCode.InvalidOrExpiredOtpCode) => "invalid_or_expired_code",
            nameof(IdentityDomainErrorCode.MaxOtpAttemptsExceeded) => "max_attempts_exceeded",
            _ => string.Empty
        };

        return !string.IsNullOrEmpty(error);
    }

    private static bool SameOrigin(Uri left, Uri right)
        => string.Equals(left.Scheme, right.Scheme, StringComparison.OrdinalIgnoreCase)
            && string.Equals(left.Host, right.Host, StringComparison.OrdinalIgnoreCase)
            && left.Port == right.Port;

    private static bool IsKnownApp(string? app)
        => !string.IsNullOrWhiteSpace(app)
            && app.Trim().ToLowerInvariant() is "admin" or "seller" or "buyer";

    private static string NormalizeApp(string app)
        => app.Trim().ToLowerInvariant();

    private static bool IsSafeReturnUrl(string? returnUrl)
    {
        if (string.IsNullOrWhiteSpace(returnUrl))
            return true;

        if (Uri.TryCreate(returnUrl, UriKind.Relative, out _))
            return returnUrl.StartsWith('/')
                && (returnUrl.Length == 1 || returnUrl[1] is not '/' and not '\\');

        return false;
    }
}
