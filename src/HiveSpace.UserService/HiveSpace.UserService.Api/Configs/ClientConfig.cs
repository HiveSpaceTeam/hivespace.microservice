// Config.cs
namespace HiveSpace.UserService.Api.Configs;

public record ClientConfig
{
    public string ClientId { get; init; } = string.Empty;
    public string ClientName { get; init; } = string.Empty;
    public string ClientUri { get; init; } = string.Empty;
    public string ClientSecret { get; init; } = string.Empty;
    public bool RequireClientSecret { get; init; }
    public List<string> AllowedGrantTypes { get; init; } = [];
    public bool AllowAccessTokensViaBrowser { get; init; }
    public bool RequireConsent { get; init; }
    public bool AllowOfflineAccess { get; init; }
    public bool AlwaysIncludeUserClaimsInIdToken { get; init; }
    public bool RequirePkce { get; init; }
    public List<string> RedirectUris { get; init; } = [];
    public List<string> PostLogoutRedirectUris { get; init; } = [];
    public List<string> AllowedCorsOrigins { get; init; } = [];
    public List<string> AllowedScopes { get; init; } = [];
    public int AccessTokenLifetime { get; init; }
    public int IdentityTokenLifetime { get; init; }
    // Refresh token settings
    public bool? UseRefreshTokens { get; init; }
    public int? AbsoluteRefreshTokenLifetime { get; init; }
    public int? SlidingRefreshTokenLifetime { get; init; }
    public string? RefreshTokenExpiration { get; init; }
}
