using System.Security.Claims;
using HiveSpace.Core.Exceptions;
using HiveSpace.Core.Exceptions.Models;
using HiveSpace.Core.Helpers;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace HiveSpace.YarpApiGateway.Middleware;

public class SessionForwardingMiddleware(
    RequestDelegate next,
    IConfiguration configuration,
    TokenValidationParameters tokenValidationParameters,
    ILogger<SessionForwardingMiddleware> logger)
{
    private readonly JsonWebTokenHandler _tokenHandler = new() { MapInboundClaims = false };
    private readonly string _accessCookieName = configuration.GetValue("BrowserSession:AccessTokenCookieName", "__Host-HiveSpace.AccessToken");
    private readonly string _refreshCookieName = configuration.GetValue("BrowserSession:RefreshTokenCookieName", "__Host-HiveSpace.RefreshToken");
    private readonly string _csrfCookieName = configuration.GetValue("BrowserSession:CsrfCookieName", "HiveSpace.Csrf");

    public async Task InvokeAsync(HttpContext context)
    {
        var originalAuthorization = context.Request.Headers.Authorization;

        if (context.Request.Cookies.TryGetValue(_accessCookieName, out var accessToken)
            && !string.IsNullOrWhiteSpace(accessToken))
        {
            context.Request.Headers.Authorization = $"Bearer {accessToken}";
            var result = await _tokenHandler.ValidateTokenAsync(accessToken, tokenValidationParameters.Clone());

            if (result.IsValid && result.ClaimsIdentity is not null)
            {
                context.User = new ClaimsPrincipal(result.ClaimsIdentity);
            }
            else if (AllowsInvalidAccessCookieBypass(context.Request))
            {
                logger.LogDebug(
                    result.Exception,
                    "Invalid browser access token was ignored for bypassable request {Method} {Path}.",
                    context.Request.Method,
                    context.Request.Path);
                RestoreAuthorizationHeader(context.Request, originalAuthorization);
            }
            else
            {
                logger.LogWarning(
                    result.Exception,
                    "Invalid browser access token rejected for request {Method} {Path}.",
                    context.Request.Method,
                    context.Request.Path);
                await WriteInvalidSessionAsync(context);
                return;
            }
        }

        if (!IsBrowserSessionEndpoint(context.Request.Path))
            StripBrowserAuthCookies(context.Request);

        await next(context);
    }

    private void StripBrowserAuthCookies(HttpRequest request)
    {
        if (!request.Headers.TryGetValue("Cookie", out var cookieHeader))
            return;

        var retainedCookies = cookieHeader
            .SelectMany(value => value?.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries) ?? [])
            .Where(cookie =>
                !cookie.StartsWith($"{_accessCookieName}=", StringComparison.OrdinalIgnoreCase)
                && !cookie.StartsWith($"{_refreshCookieName}=", StringComparison.OrdinalIgnoreCase)
                && !cookie.StartsWith($"{_csrfCookieName}=", StringComparison.OrdinalIgnoreCase))
            .ToArray();

        if (retainedCookies.Length == 0)
            request.Headers.Remove("Cookie");
        else
            request.Headers.Cookie = string.Join("; ", retainedCookies);
    }

    private static bool IsBrowserSessionEndpoint(PathString path)
        => path.Equals("/api/v1/accounts/session/refresh", StringComparison.OrdinalIgnoreCase)
            || path.Equals("/api/v1/accounts/logout", StringComparison.OrdinalIgnoreCase);

    private static bool AllowsInvalidAccessCookieBypass(HttpRequest request)
    {
        if (IsBrowserSessionEndpoint(request.Path))
            return true;

        if (HttpMethods.IsGet(request.Method)
            || HttpMethods.IsHead(request.Method)
            || HttpMethods.IsOptions(request.Method))
            return true;

        return request.Path.Equals("/api/v1/accounts/login", StringComparison.OrdinalIgnoreCase)
            || request.Path.Equals("/api/v1/accounts/register", StringComparison.OrdinalIgnoreCase);
    }

    private static void RestoreAuthorizationHeader(HttpRequest request, StringValues originalAuthorization)
    {
        if (StringValues.IsNullOrEmpty(originalAuthorization))
            request.Headers.Remove("Authorization");
        else
            request.Headers.Authorization = originalAuthorization;
    }

    private static async Task WriteInvalidSessionAsync(HttpContext context)
    {
        var response = ExceptionResponseFactory.CreateResponse(
            new UnauthorizedException([new Error(CommonErrorCode.InvalidArgument, "accessToken")]));

        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        await context.Response.WriteAsJsonAsync(response);
    }
}
