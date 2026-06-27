using System.Text.Json;
using HiveSpace.Core.Exceptions;
using HiveSpace.Core.Exceptions.Models;
using HiveSpace.Core.Helpers;

namespace HiveSpace.YarpApiGateway.Middleware;

public class CsrfValidationMiddleware(
    RequestDelegate next,
    IConfiguration configuration)
{
    private const string CsrfTokenPurpose = "hivespace-browser-csrf-token";
    private const string RefreshTokenPurpose = "hivespace-browser-refresh-token";
    private const string SessionIdClaim = "sid";
    private readonly string _accessCookieName = configuration.GetValue("BrowserSession:AccessTokenCookieName", "__Host-HiveSpace.AccessToken");
    private readonly string _refreshCookieName = configuration.GetValue("BrowserSession:RefreshTokenCookieName", "__Host-HiveSpace.RefreshToken");
    private readonly string _csrfCookieName = configuration.GetValue("BrowserSession:CsrfCookieName", "HiveSpace.Csrf");
    private readonly string _csrfHeaderName = configuration.GetValue("BrowserSession:CsrfHeaderName", "X-HiveSpace-CSRF");
    private readonly string _signingKey = configuration.GetValue<string>("BrowserSession:SigningKey")
        ?? throw new InvalidOperationException("BrowserSession.SigningKey is required.");

    public async Task InvokeAsync(HttpContext context)
    {
        if (!RequiresCsrfValidation(context.Request))
        {
            await next(context);
            return;
        }

        var hasAccessCookie = context.Request.Cookies.TryGetValue(_accessCookieName, out var accessToken)
            && !string.IsNullOrWhiteSpace(accessToken);
        var hasRefreshCookie = context.Request.Cookies.TryGetValue(_refreshCookieName, out var refreshToken)
            && !string.IsNullOrWhiteSpace(refreshToken);
        if (!hasAccessCookie && !hasRefreshCookie)
        {
            await next(context);
            return;
        }

        var sessionId = context.User.FindFirst(SessionIdClaim)?.Value
            ?? TryReadRefreshSessionId(refreshToken);
        if (string.IsNullOrWhiteSpace(sessionId)
            || !context.Request.Cookies.TryGetValue(_csrfCookieName, out var csrfCookie)
            || string.IsNullOrWhiteSpace(csrfCookie)
            || !context.Request.Headers.TryGetValue(_csrfHeaderName, out var submittedToken)
            || !ValidateCsrfToken(csrfCookie, submittedToken.FirstOrDefault(), sessionId))
        {
            await WriteCsrfFailureAsync(context);
            return;
        }

        await next(context);
    }

    private static bool RequiresCsrfValidation(HttpRequest request)
    {
        if (HttpMethods.IsGet(request.Method)
            || HttpMethods.IsHead(request.Method)
            || HttpMethods.IsOptions(request.Method))
            return false;

        if (request.Path.Equals("/api/v1/accounts/login", StringComparison.OrdinalIgnoreCase)
            || request.Path.Equals("/api/v1/accounts/register", StringComparison.OrdinalIgnoreCase)
            || request.Path.Equals("/api/v1/accounts/otp/request", StringComparison.OrdinalIgnoreCase)
            || request.Path.Equals("/api/v1/accounts/otp/verify", StringComparison.OrdinalIgnoreCase)
            || request.Path.Equals("/api/v1/accounts/email-verification/resend", StringComparison.OrdinalIgnoreCase)
            || request.Path.Equals("/api/v1/accounts/email-verification/verify", StringComparison.OrdinalIgnoreCase))
            return false;

        return true;
    }

    private string? TryReadRefreshSessionId(string? refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken)
            || !BrowserSessionTokenSigner.TryReadPayload(RefreshTokenPurpose, refreshToken, _signingKey, out var payloadJson))
        {
            return null;
        }

        try
        {
            var payload = JsonSerializer.Deserialize<RefreshTokenPayload>(payloadJson);
            if (payload is null
                || DateTimeOffset.FromUnixTimeSeconds(payload.RefreshExpiresAtUnixSeconds) <= DateTimeOffset.UtcNow)
            {
                return null;
            }

            return payload.SessionId;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private bool ValidateCsrfToken(string expectedToken, string? submittedToken, string sessionId)
    {
        if (string.IsNullOrWhiteSpace(submittedToken)
            || !string.Equals(expectedToken, submittedToken, StringComparison.Ordinal)
            || !BrowserSessionTokenSigner.TryReadPayload(CsrfTokenPurpose, submittedToken, _signingKey, out var payloadJson))
            return false;

        try
        {
            var payload = JsonSerializer.Deserialize<CsrfTokenPayload>(payloadJson);
            if (payload is null)
                return false;

            return string.Equals(payload.SessionId, sessionId, StringComparison.Ordinal)
                && DateTimeOffset.FromUnixTimeSeconds(payload.ExpiresAtUnixSeconds) > DateTimeOffset.UtcNow;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static async Task WriteCsrfFailureAsync(HttpContext context)
    {
        var response = ExceptionResponseFactory.CreateResponse(
            new BadRequestException([new Error(CommonErrorCode.CsrfValidationFailed, "XHiveSpaceCsrf")]));

        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        await context.Response.WriteAsJsonAsync(response);
    }

    private sealed record CsrfTokenPayload(
        string SessionId,
        long IssuedAtUnixSeconds,
        long ExpiresAtUnixSeconds);

    private sealed record RefreshTokenPayload(
        string SessionId,
        Guid UserId,
        string RefreshHandle,
        string App,
        string? SecurityStamp,
        long IssuedAtUnixSeconds,
        long RefreshExpiresAtUnixSeconds);
}
