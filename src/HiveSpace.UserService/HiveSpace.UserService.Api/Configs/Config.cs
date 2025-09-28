using Duende.IdentityServer.Models;

namespace HiveSpace.UserService.Api.Configs;

public static class Config
{
    public static IEnumerable<IdentityResource> IdentityResources =>
        [
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
        ];

    public static IEnumerable<ApiScope> ApiScopes =>
        [
            new ApiScope("user.fullaccess", "User API Full Access"),
            new ApiScope("order.fullaccess", "Order API Full Access"),
            new ApiScope("basket.fullaccess", "Basket API Full Access"),
            new ApiScope("catalog.fullaccess", "Catalog API Full Access"),
            new ApiScope("hivespace-backend.fullaccess", "Hivespace Backend API Full access")
        ];

    public static IEnumerable<ApiResource> ApiResources =>
        [
            new ApiResource("user", "User API")
            {
                Scopes = { "user.fullaccess" }
            },
            new ApiResource("order", "Order API")
            {
                Scopes = { "order.fullaccess" },
                UserClaims = { "sub", "name", "email", "phone_number" }
            },
            new ApiResource("basket", "Basket API")
            {
                Scopes = { "basket.fullaccess" },
                UserClaims = { "sub", "name", "email" }
            },
            new ApiResource("catalog", "Catalog API")
            {
                Scopes = { "catalog.fullaccess" },
                UserClaims = { "sub", "name", "email" }
            },
            new ApiResource("hivespace-backend", "HiveSpace Backend API")
            {
                Scopes = { "hivespace-backend.fullaccess" },
                UserClaims = { "sub", "name", "email", "phone_number" }
            }
        ];

    public static IEnumerable<Client> GetClients(IConfiguration configuration)
    {
        var clients = new List<Client>();
        var clientsSection = configuration.GetSection("Clients");

        // Admin Portal Client (full config)
        var adminPortalConfig = clientsSection.GetSection("adminportal").Get<ClientConfig>();
        if (adminPortalConfig != null)
        {
            var adminPortalClient = new Client
            {
                ClientId = adminPortalConfig.ClientId,
                ClientName = adminPortalConfig.ClientName,
                ClientUri = adminPortalConfig.ClientUri,
                ClientSecrets = !string.IsNullOrEmpty(adminPortalConfig.ClientSecret)
                    ? new List<Secret> { new Secret(adminPortalConfig.ClientSecret.Sha256()) }
                    : new List<Secret>(),
                RequireClientSecret = adminPortalConfig.RequireClientSecret,
                AllowedGrantTypes = adminPortalConfig.AllowedGrantTypes ?? ["authorization_code"],
                AllowAccessTokensViaBrowser = adminPortalConfig.AllowAccessTokensViaBrowser,
                RequireConsent = adminPortalConfig.RequireConsent,
                AllowOfflineAccess = adminPortalConfig.AllowOfflineAccess,
                AlwaysIncludeUserClaimsInIdToken = adminPortalConfig.AlwaysIncludeUserClaimsInIdToken,
                RequirePkce = adminPortalConfig.RequirePkce,
                RedirectUris = adminPortalConfig.RedirectUris ?? [],
                PostLogoutRedirectUris = adminPortalConfig.PostLogoutRedirectUris ?? [],
                AllowedCorsOrigins = adminPortalConfig.AllowedCorsOrigins ?? [],
                AllowedScopes = adminPortalConfig.AllowedScopes ?? [],
                AccessTokenLifetime = adminPortalConfig.AccessTokenLifetime,
                IdentityTokenLifetime = adminPortalConfig.IdentityTokenLifetime
            };

            // Ensure OIDC identity scopes are present for interactive flows
            var oidcScopes = new[] { "openid", "profile" };
            adminPortalClient.AllowedScopes = (adminPortalClient.AllowedScopes ?? new List<string>())
                .Concat(oidcScopes)
                .Distinct()
                .ToList();

            // Ensure offline_access scope is present when AllowOfflineAccess is true
            if (adminPortalConfig.AllowOfflineAccess)
            {
                adminPortalClient.AllowedScopes ??= new List<string>();
                var allowedScopes = adminPortalClient.AllowedScopes.ToList();
                if (!allowedScopes.Contains("offline_access"))
                {
                    allowedScopes.Add("offline_access");
                    adminPortalClient.AllowedScopes = allowedScopes;
                }
            }

            clients.Add(adminPortalClient);
        }

        // API Testing Client (minimal config)
        // Seller Center Client
        var sellerCenterConfig = clientsSection.GetSection("sellercenter").Get<ClientConfig>();
        if (sellerCenterConfig != null)
        {
            var sellerCenterClient = new Client
            {
                ClientId = sellerCenterConfig.ClientId,
                ClientName = sellerCenterConfig.ClientName,
                ClientUri = sellerCenterConfig.ClientUri,
                ClientSecrets = !string.IsNullOrEmpty(sellerCenterConfig.ClientSecret)
                    ? new List<Secret> { new Secret(sellerCenterConfig.ClientSecret.Sha256()) }
                    : new List<Secret>(),
                RequireClientSecret = sellerCenterConfig.RequireClientSecret,
                AllowedGrantTypes = sellerCenterConfig.AllowedGrantTypes ?? new List<string> { "authorization_code" },
                AllowAccessTokensViaBrowser = sellerCenterConfig.AllowAccessTokensViaBrowser,
                RequireConsent = sellerCenterConfig.RequireConsent,
                AllowOfflineAccess = sellerCenterConfig.AllowOfflineAccess,
                AlwaysIncludeUserClaimsInIdToken = sellerCenterConfig.AlwaysIncludeUserClaimsInIdToken,
                RequirePkce = sellerCenterConfig.RequirePkce,
                RedirectUris = sellerCenterConfig.RedirectUris ?? new List<string>(),
                PostLogoutRedirectUris = sellerCenterConfig.PostLogoutRedirectUris ?? new List<string>(),
                AllowedCorsOrigins = sellerCenterConfig.AllowedCorsOrigins ?? new List<string>(),
                AllowedScopes = sellerCenterConfig.AllowedScopes ?? new List<string>(),
                AccessTokenLifetime = sellerCenterConfig.AccessTokenLifetime,
                IdentityTokenLifetime = sellerCenterConfig.IdentityTokenLifetime,
            };

            var oidcScopesSeller = new[] { "openid", "profile" };
            sellerCenterClient.AllowedScopes = (sellerCenterClient.AllowedScopes ?? new List<string>())
                .Concat(oidcScopesSeller)
                .Distinct()
                .ToList();

            if (sellerCenterConfig.AllowOfflineAccess)
            {
                sellerCenterClient.AllowedScopes ??= new List<string>();
                var allowedScopes = sellerCenterClient.AllowedScopes.ToList();
                if (!allowedScopes.Contains("offline_access"))
                {
                    allowedScopes.Add("offline_access");
                    sellerCenterClient.AllowedScopes = allowedScopes;
                }

                // Configure refresh token behavior for SPA using refresh tokens
                sellerCenterClient.RefreshTokenUsage = TokenUsage.OneTimeOnly;
                // default to sliding expiration unless configured
                sellerCenterClient.RefreshTokenExpiration = TokenExpiration.Sliding;
                if (sellerCenterConfig.AbsoluteRefreshTokenLifetime.HasValue && sellerCenterConfig.AbsoluteRefreshTokenLifetime.Value > 0)
                    sellerCenterClient.AbsoluteRefreshTokenLifetime = sellerCenterConfig.AbsoluteRefreshTokenLifetime.Value;
                if (sellerCenterConfig.SlidingRefreshTokenLifetime.HasValue && sellerCenterConfig.SlidingRefreshTokenLifetime.Value > 0)
                    sellerCenterClient.SlidingRefreshTokenLifetime = sellerCenterConfig.SlidingRefreshTokenLifetime.Value;
            }

            clients.Add(sellerCenterClient);
        }

        // Web UI client
        var webUiConfig = clientsSection.GetSection("webui").Get<ClientConfig>();
        if (webUiConfig != null)
        {
            var webUiClient = new Client
            {
                ClientId = webUiConfig.ClientId,
                ClientName = webUiConfig.ClientName,
                ClientUri = webUiConfig.ClientUri,
                ClientSecrets = !string.IsNullOrEmpty(webUiConfig.ClientSecret)
                    ? new List<Secret> { new Secret(webUiConfig.ClientSecret.Sha256()) }
                    : new List<Secret>(),
                RequireClientSecret = webUiConfig.RequireClientSecret,
                AllowedGrantTypes = webUiConfig.AllowedGrantTypes ?? new List<string> { "authorization_code" },
                AllowAccessTokensViaBrowser = webUiConfig.AllowAccessTokensViaBrowser,
                RequireConsent = webUiConfig.RequireConsent,
                AllowOfflineAccess = webUiConfig.AllowOfflineAccess,
                AlwaysIncludeUserClaimsInIdToken = webUiConfig.AlwaysIncludeUserClaimsInIdToken,
                RequirePkce = webUiConfig.RequirePkce,
                RedirectUris = webUiConfig.RedirectUris ?? new List<string>(),
                PostLogoutRedirectUris = webUiConfig.PostLogoutRedirectUris ?? new List<string>(),
                AllowedCorsOrigins = webUiConfig.AllowedCorsOrigins ?? new List<string>(),
                AllowedScopes = webUiConfig.AllowedScopes ?? new List<string>(),
                AccessTokenLifetime = webUiConfig.AccessTokenLifetime,
                IdentityTokenLifetime = webUiConfig.IdentityTokenLifetime
            };

            var oidcScopesWeb = new[] { "openid", "profile" };
            webUiClient.AllowedScopes = (webUiClient.AllowedScopes ?? new List<string>())
                .Concat(oidcScopesWeb)
                .Distinct()
                .ToList();

            if (webUiConfig.AllowOfflineAccess)
            {
                webUiClient.AllowedScopes ??= new List<string>();
                var allowedScopes = webUiClient.AllowedScopes.ToList();
                if (!allowedScopes.Contains("offline_access"))
                {
                    allowedScopes.Add("offline_access");
                    webUiClient.AllowedScopes = allowedScopes;
                }

                // Configure refresh token behavior for SPA using refresh tokens
                webUiClient.RefreshTokenUsage = TokenUsage.OneTimeOnly;
                webUiClient.RefreshTokenExpiration = TokenExpiration.Sliding;
                if (webUiConfig.AbsoluteRefreshTokenLifetime.HasValue && webUiConfig.AbsoluteRefreshTokenLifetime.Value > 0)
                    webUiClient.AbsoluteRefreshTokenLifetime = webUiConfig.AbsoluteRefreshTokenLifetime.Value;
                if (webUiConfig.SlidingRefreshTokenLifetime.HasValue && webUiConfig.SlidingRefreshTokenLifetime.Value > 0)
                    webUiClient.SlidingRefreshTokenLifetime = webUiConfig.SlidingRefreshTokenLifetime.Value;
            }

            clients.Add(webUiClient);
        }
        return clients;
    }
}
