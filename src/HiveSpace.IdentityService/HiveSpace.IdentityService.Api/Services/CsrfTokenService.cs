using System.Text.Json;
using HiveSpace.Core.Exceptions;
using HiveSpace.Core.Exceptions.Models;
using HiveSpace.Core.Helpers;
using HiveSpace.IdentityService.Core.Exceptions;
using HiveSpace.IdentityService.Core.Features.AccountSessions.Services;

namespace HiveSpace.IdentityService.Api.Services;

public class CsrfTokenService(
    IHttpContextAccessor httpContextAccessor,
    IConfiguration configuration)
    : ICsrfTokenService
{
    private const string CsrfTokenPurpose = "hivespace-browser-csrf-token";
    private readonly string _cookieName = configuration.GetValue("BrowserSession:CsrfCookieName", "HiveSpace.Csrf");
    private readonly string _signingKey = configuration.GetValue<string>("BrowserSession:SigningKey")
        ?? throw new ConfigurationException([new Error(IdentityDomainErrorCode.InvalidConfiguration, "BrowserSession.SigningKey")]);

    public string Issue(string sessionId, DateTimeOffset expiresAt)
    {
        var issuedAt = DateTimeOffset.UtcNow;
        var payload = new CsrfTokenPayload(
            sessionId,
            issuedAt.ToUnixTimeSeconds(),
            expiresAt.ToUnixTimeSeconds());
        var token = BrowserSessionTokenSigner.Sign(CsrfTokenPurpose, JsonSerializer.Serialize(payload), _signingKey);

        var httpContext = httpContextAccessor.HttpContext
            ?? throw new UnauthorizedException([new Error(IdentityDomainErrorCode.InvalidSession, "session")]);

        httpContext.Response.Cookies.Append(_cookieName, token, new CookieOptions
        {
            HttpOnly = false,
            Secure = true,
            SameSite = SameSiteMode.None,
            Path = "/",
            Expires = expiresAt,
            MaxAge = expiresAt - DateTimeOffset.UtcNow,
            IsEssential = true
        });

        return token;
    }

    public void Clear()
    {
        var httpContext = httpContextAccessor.HttpContext;
        httpContext?.Response.Cookies.Delete(_cookieName, new CookieOptions
        {
            Path = "/",
            Secure = true,
            SameSite = SameSiteMode.None
        });
    }

    private sealed record CsrfTokenPayload(
        string SessionId,
        long IssuedAtUnixSeconds,
        long ExpiresAtUnixSeconds);
}
