using System.Security.Cryptography;
using System.Text.Json;
using HiveSpace.Core.Exceptions;
using HiveSpace.Core.Exceptions.Models;
using HiveSpace.Core.Helpers;
using HiveSpace.IdentityService.Api.Configs;
using HiveSpace.IdentityService.Core.Exceptions;
using HiveSpace.IdentityService.Core.Interfaces.Services;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.WebUtilities;

namespace HiveSpace.IdentityService.Api.Services;

public class PendingGoogleLinkCookieStore(
    IHttpContextAccessor httpContextAccessor,
    IConfiguration configuration,
    IOptions<GoogleExternalAuthOptions> options)
    : IPendingGoogleLinkStore
{
    private const string CookieName = "__Host-HiveSpace.PendingGoogleLink";
    private const string TokenPurpose = "hivespace-pending-google-link";

    private readonly string _signingKey = configuration.GetValue<string>("BrowserSession:SigningKey")
        ?? throw new ConfigurationException([new Error(IdentityDomainErrorCode.InvalidConfiguration, "BrowserSession.SigningKey")]);

    public Task<PendingGoogleLinkState> CreateAsync(
        PendingGoogleLinkCreateRequest request,
        CancellationToken cancellationToken = default)
    {
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(options.Value.PendingLinkLifetimeMinutes);
        var state = new PendingGoogleLinkState(
            request.Provider,
            request.ProviderKey,
            request.ProviderDisplayName,
            request.VerifiedEmail,
            request.TargetAccountId,
            request.App,
            request.ReturnUrl,
            request.Culture,
            request.ExpiresAt < expiresAt ? request.ExpiresAt : expiresAt,
            WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(32)));

        var signedValue = BrowserSessionTokenSigner.Sign(TokenPurpose, JsonSerializer.Serialize(state), _signingKey);
        var httpContext = httpContextAccessor.HttpContext
            ?? throw new UnauthorizedException([new Error(IdentityDomainErrorCode.InvalidSession, "session")]);

        httpContext.Response.Cookies.Append(CookieName, signedValue, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Path = "/",
            Expires = state.ExpiresAt,
            MaxAge = state.ExpiresAt - DateTimeOffset.UtcNow,
            IsEssential = true
        });

        return Task.FromResult(state);
    }

    public Task<PendingGoogleLinkState> GetRequiredAsync(string linkToken, CancellationToken cancellationToken = default)
    {
        var httpContext = httpContextAccessor.HttpContext
            ?? throw new UnauthorizedException([new Error(IdentityDomainErrorCode.InvalidSession, "session")]);

        if (!httpContext.Request.Cookies.TryGetValue(CookieName, out var signedValue)
            || string.IsNullOrWhiteSpace(signedValue)
            || !BrowserSessionTokenSigner.TryReadPayload(TokenPurpose, signedValue, _signingKey, out var payloadJson))
        {
            throw new UnauthorizedException([new Error(IdentityDomainErrorCode.PendingGoogleLinkNotFound, nameof(linkToken))]);
        }

        PendingGoogleLinkState? state;
        try
        {
            state = JsonSerializer.Deserialize<PendingGoogleLinkState>(payloadJson);
        }
        catch (JsonException)
        {
            state = null;
        }

        if (state is null
            || state.ExpiresAt <= DateTimeOffset.UtcNow
            || !string.Equals(state.LinkToken, linkToken, StringComparison.Ordinal))
        {
            throw new UnauthorizedException([new Error(IdentityDomainErrorCode.PendingGoogleLinkExpired, nameof(linkToken))]);
        }

        return Task.FromResult(state);
    }

    public Task ClearAsync(CancellationToken cancellationToken = default)
    {
        httpContextAccessor.HttpContext?.Response.Cookies.Delete(CookieName, new CookieOptions
        {
            Path = "/",
            Secure = true,
            SameSite = SameSiteMode.None
        });

        return Task.CompletedTask;
    }
}
