using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Duende.IdentityServer;
using HiveSpace.Core.Exceptions;
using HiveSpace.Core.Exceptions.Models;
using HiveSpace.Core.Helpers;
using HiveSpace.IdentityService.Core.Exceptions;
using HiveSpace.IdentityService.Core.Features.AccountSessions.Services;
using HiveSpace.IdentityService.Core.DomainModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;

namespace HiveSpace.IdentityService.Api.Services;

public class TokenCookieService(
    IHttpContextAccessor httpContextAccessor,
    IIdentityServerTools identityServerTools,
    UserManager<ApplicationUser> userManager,
    IConfiguration configuration)
    : ITokenCookieService
{
    private const string RefreshTokenPurpose = "hivespace-browser-refresh-token";
    private const string TokenProvider = "HiveSpace.BrowserSession";
    private const string SessionIdClaim = "sid";
    private const string SecurityStampClaim = "security_stamp";

    private readonly string _accessCookieName = configuration.GetValue("BrowserSession:AccessTokenCookieName", "__Host-HiveSpace.AccessToken");
    private readonly string _refreshCookieName = configuration.GetValue("BrowserSession:RefreshTokenCookieName", "__Host-HiveSpace.RefreshToken");
    private readonly string _signingKey = configuration.GetValue<string>("BrowserSession:SigningKey")
        ?? throw new ConfigurationException([new Error(IdentityDomainErrorCode.InvalidConfiguration, "BrowserSession.SigningKey")]);

    public async Task<TokenCookieIssueResult> IssueAsync(
        ApplicationUser user,
        string app,
        CancellationToken cancellationToken = default)
    {
        var sessionId = Guid.NewGuid().ToString("N");
        return await IssueSessionCookiesAsync(user, app, sessionId, cancellationToken);
    }

    public async Task<TokenCookieIssueResult> RefreshAsync(
        BrowserRefreshSession currentSession,
        ApplicationUser user,
        string app,
        CancellationToken cancellationToken = default)
    {
        var storedHandleHash = await userManager.GetAuthenticationTokenAsync(user, TokenProvider, currentSession.SessionId);
        if (!FixedTimeEquals(storedHandleHash, HashHandle(currentSession.RefreshHandle)))
            throw new UnauthorizedException([new Error(IdentityDomainErrorCode.InvalidSession, "session")]);

        return await IssueSessionCookiesAsync(user, app, currentSession.SessionId, cancellationToken);
    }

    private async Task<TokenCookieIssueResult> IssueSessionCookiesAsync(
        ApplicationUser user,
        string app,
        string sessionId,
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var refreshHandle = CreateHandle();
        var accessExpiresAt = now.AddMinutes(configuration.GetValue("BrowserSession:AccessTokenMinutes", 15));
        var refreshExpiresAt = now.AddDays(configuration.GetValue("BrowserSession:RefreshTokenDays", 30));

        var accessToken = await IssueAccessTokenAsync(user, app, sessionId, accessExpiresAt, cancellationToken);
        var refreshToken = await IssueRefreshTokenAsync(user, app, sessionId, refreshHandle, now, refreshExpiresAt);

        SetCookie(_accessCookieName, accessToken, accessExpiresAt, httpOnly: true);
        SetCookie(_refreshCookieName, refreshToken, refreshExpiresAt, httpOnly: true);

        return new TokenCookieIssueResult(sessionId, accessExpiresAt, refreshExpiresAt);
    }

    public Task<BrowserRefreshSession> GetRequiredRefreshSessionAsync(CancellationToken cancellationToken = default)
    {
        var httpContext = httpContextAccessor.HttpContext
            ?? throw new UnauthorizedException([new Error(IdentityDomainErrorCode.InvalidSession, "session")]);

        if (!httpContext.Request.Cookies.TryGetValue(_refreshCookieName, out var refreshToken)
            || string.IsNullOrWhiteSpace(refreshToken)
            || !TryReadRefreshToken(refreshToken, out var session)
            || session.RefreshExpiresAt <= DateTimeOffset.UtcNow)
        {
            throw new UnauthorizedException([new Error(IdentityDomainErrorCode.SessionExpired, "session")]);
        }

        return Task.FromResult(session);
    }

    public async Task ClearAsync(CancellationToken cancellationToken = default)
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext?.Request.Cookies.TryGetValue(_refreshCookieName, out var refreshToken) == true
            && TryReadRefreshToken(refreshToken, out var session))
        {
            var user = await userManager.FindByIdAsync(session.UserId.ToString());
            if (user is not null)
                await userManager.RemoveAuthenticationTokenAsync(user, TokenProvider, session.SessionId);
        }

        DeleteCookie(_accessCookieName);
        DeleteCookie(_refreshCookieName);
    }

    private async Task<string> IssueAccessTokenAsync(
        ApplicationUser user,
        string app,
        string sessionId,
        DateTimeOffset accessExpiresAt,
        CancellationToken cancellationToken)
    {
        var role = string.IsNullOrWhiteSpace(user.RoleName) ? "Buyer" : user.RoleName;
        var claims = new List<Claim>
        {
            new(SessionIdClaim, sessionId),
            new("sub", user.Id.ToString()),
            new("email", user.Email ?? string.Empty),
            new("name", user.UserName ?? user.Email ?? string.Empty),
            new("username", user.UserName ?? string.Empty),
            new("email_verified", user.EmailConfirmed.ToString().ToLowerInvariant(), ClaimValueTypes.Boolean),
            new("role", role),
            new("userStatus", ((int)user.Status).ToString()),
            new(SecurityStampClaim, await userManager.GetSecurityStampAsync(user))
        };

        if (user.StoreId.HasValue)
            claims.Add(new Claim("store_id", user.StoreId.Value.ToString()));

        if (!string.IsNullOrWhiteSpace(user.PhoneNumber))
            claims.Add(new Claim("phone_number", user.PhoneNumber));

        var scopes = GetConfiguredValues("BrowserSession:AccessTokenScopes", DefaultScopes());
        var audiences = GetConfiguredValues("BrowserSession:AccessTokenAudiences", DefaultAudiences());
        var clientId = app.Trim().ToLowerInvariant() switch
        {
            "admin" => "adminportal",
            "seller" => "sellercenter",
            _ => "storefront"
        };

        var lifetimeSeconds = Math.Max(60, (int)(accessExpiresAt - DateTimeOffset.UtcNow).TotalSeconds);
        return await identityServerTools.IssueClientJwtAsync(clientId, lifetimeSeconds, scopes, audiences, claims);
    }

    private async Task<string> IssueRefreshTokenAsync(
        ApplicationUser user,
        string app,
        string sessionId,
        string refreshHandle,
        DateTimeOffset issuedAt,
        DateTimeOffset refreshExpiresAt)
    {
        await userManager.SetAuthenticationTokenAsync(user, TokenProvider, sessionId, HashHandle(refreshHandle));
        var securityStamp = await userManager.GetSecurityStampAsync(user);
        var payload = new RefreshTokenPayload(
            sessionId,
            user.Id,
            refreshHandle,
            app.Trim().ToLowerInvariant(),
            securityStamp,
            issuedAt.ToUnixTimeSeconds(),
            refreshExpiresAt.ToUnixTimeSeconds());

        return BrowserSessionTokenSigner.Sign(RefreshTokenPurpose, JsonSerializer.Serialize(payload), _signingKey);
    }

    private bool TryReadRefreshToken(string refreshToken, out BrowserRefreshSession session)
    {
        session = default!;

        if (!BrowserSessionTokenSigner.TryReadPayload(RefreshTokenPurpose, refreshToken, _signingKey, out var payloadJson))
            return false;

        try
        {
            var payload = JsonSerializer.Deserialize<RefreshTokenPayload>(payloadJson);
            if (payload is null
                || payload.UserId == Guid.Empty
                || string.IsNullOrWhiteSpace(payload.SessionId)
                || string.IsNullOrWhiteSpace(payload.RefreshHandle)
                || string.IsNullOrWhiteSpace(payload.App))
            {
                return false;
            }

            session = new BrowserRefreshSession(
                payload.SessionId,
                payload.UserId,
                payload.RefreshHandle,
                payload.App,
                DateTimeOffset.FromUnixTimeSeconds(payload.RefreshExpiresAtUnixSeconds),
                payload.SecurityStamp,
                DateTimeOffset.FromUnixTimeSeconds(payload.IssuedAtUnixSeconds));
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
        catch (ArgumentOutOfRangeException)
        {
            return false;
        }
    }

    private void SetCookie(string cookieName, string value, DateTimeOffset expiresAt, bool httpOnly)
    {
        var httpContext = httpContextAccessor.HttpContext
            ?? throw new UnauthorizedException([new Error(IdentityDomainErrorCode.InvalidSession, "session")]);

        httpContext.Response.Cookies.Append(cookieName, value, new CookieOptions
        {
            HttpOnly = httpOnly,
            Secure = true,
            SameSite = SameSiteMode.None,
            Path = "/",
            Expires = expiresAt,
            MaxAge = expiresAt - DateTimeOffset.UtcNow,
            IsEssential = true
        });
    }

    private void DeleteCookie(string cookieName)
    {
        httpContextAccessor.HttpContext?.Response.Cookies.Delete(cookieName, new CookieOptions
        {
            Path = "/",
            Secure = true,
            SameSite = SameSiteMode.None
        });
    }

    private string[] GetConfiguredValues(string key, string[] fallback)
    {
        var values = configuration.GetSection(key).Get<string[]>();
        return values is { Length: > 0 } ? values : fallback;
    }

    private static string CreateHandle()
        => WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(32));

    private static string HashHandle(string handle)
        => WebEncoders.Base64UrlEncode(SHA256.HashData(Encoding.UTF8.GetBytes(handle)));

    private static bool FixedTimeEquals(string? expected, string actual)
    {
        if (string.IsNullOrWhiteSpace(expected))
            return false;

        var expectedBytes = Encoding.UTF8.GetBytes(expected);
        var actualBytes = Encoding.UTF8.GetBytes(actual);
        return expectedBytes.Length == actualBytes.Length
            && CryptographicOperations.FixedTimeEquals(expectedBytes, actualBytes);
    }

    private static string[] DefaultScopes() =>
    [
        "identity.fullaccess",
        "user.fullaccess",
        "catalog.fullaccess",
        "order.fullaccess",
        "media.fullaccess",
        "payment.fullaccess",
        "notification.fullaccess"
    ];

    private static string[] DefaultAudiences() =>
    [
        "identity",
        "user",
        "catalog",
        "order",
        "media",
        "payment",
        "notification"
    ];

    private sealed record RefreshTokenPayload(
        string SessionId,
        Guid UserId,
        string RefreshHandle,
        string App,
        string? SecurityStamp,
        long IssuedAtUnixSeconds,
        long RefreshExpiresAtUnixSeconds);
}
