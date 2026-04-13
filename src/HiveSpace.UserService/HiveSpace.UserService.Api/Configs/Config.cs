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
            new ApiScope("user.fullaccess", "User API Full Access") { UserClaims = { "sub", "name", "email", "role" } },
            new ApiScope("order.fullaccess", "Order API Full Access") { UserClaims = { "sub", "name", "email", "phone_number", "role" } },
            new ApiScope("basket.fullaccess", "Basket API Full Access") { UserClaims = { "sub", "name", "email", "role" } },
            new ApiScope("catalog.fullaccess", "Catalog API Full Access") { UserClaims = { "sub", "name", "email", "role" } },
            new ApiScope("media.fullaccess", "Media API Full Access") { UserClaims = { "sub", "name", "email", "role" } },
            new ApiScope("hivespace-backend.fullaccess", "Hivespace Backend API Full access") { UserClaims = { "sub", "name", "email", "role" } },
            new ApiScope("payment.fullaccess", "Payment API Full Access") { UserClaims = { "sub", "name", "email", "role" } }
        ];

    public static IEnumerable<ApiResource> ApiResources =>
        [
            new ApiResource("user", "User API")
            {
                Scopes = { "user.fullaccess" },
                UserClaims = { "sub", "name", "email", "role" }
            },
            new ApiResource("order", "Order API")
            {
                Scopes = { "order.fullaccess" },
                UserClaims = { "sub", "name", "email", "phone_number", "role" }
            },
            new ApiResource("basket", "Basket API")
            {
                Scopes = { "basket.fullaccess" },
                UserClaims = { "sub", "name", "email", "role" }
            },
            new ApiResource("catalog", "Catalog API")
            {
                Scopes = { "catalog.fullaccess" },
                UserClaims = { "sub", "name", "email", "role" }
            },
            new ApiResource("media", "Media API")
            {
                Scopes = { "media.fullaccess" },
                UserClaims = { "sub", "name", "email", "role" }
            },
            new ApiResource("payment", "Payment API")
            {
                Scopes = { "payment.fullaccess" },
                UserClaims = { "sub", "name", "email", "role" }
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
                UpdateAccessTokenClaimsOnRefresh = true,
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
                UpdateAccessTokenClaimsOnRefresh = true,
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
                sellerCenterClient.RefreshTokenUsage = sellerCenterConfig.RefreshTokenUsage?.ToLower() switch
                {
                    "reuse" => TokenUsage.ReUse,
                    "onetimeonly" => TokenUsage.OneTimeOnly,
                    _ => TokenUsage.OneTimeOnly // Default to OneTimeOnly for security
                };
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
        var storefrontConfig = clientsSection.GetSection("storefront").Get<ClientConfig>();
        if (storefrontConfig != null)
        {
            var storefrontClient = new Client
            {
                ClientId = storefrontConfig.ClientId,
                ClientName = storefrontConfig.ClientName,
                ClientUri = storefrontConfig.ClientUri,
                ClientSecrets = !string.IsNullOrEmpty(storefrontConfig.ClientSecret)
                    ? new List<Secret> { new Secret(storefrontConfig.ClientSecret.Sha256()) }
                    : new List<Secret>(),
                RequireClientSecret = storefrontConfig.RequireClientSecret,
                AllowedGrantTypes = storefrontConfig.AllowedGrantTypes ?? new List<string> { "authorization_code" },
                AllowAccessTokensViaBrowser = storefrontConfig.AllowAccessTokensViaBrowser,
                RequireConsent = storefrontConfig.RequireConsent,
                AllowOfflineAccess = storefrontConfig.AllowOfflineAccess,
                AlwaysIncludeUserClaimsInIdToken = storefrontConfig.AlwaysIncludeUserClaimsInIdToken,
                UpdateAccessTokenClaimsOnRefresh = true,
                RequirePkce = storefrontConfig.RequirePkce,
                RedirectUris = storefrontConfig.RedirectUris ?? new List<string>(),
                PostLogoutRedirectUris = storefrontConfig.PostLogoutRedirectUris ?? new List<string>(),
                AllowedCorsOrigins = storefrontConfig.AllowedCorsOrigins ?? new List<string>(),
                AllowedScopes = storefrontConfig.AllowedScopes ?? new List<string>(),
                AccessTokenLifetime = storefrontConfig.AccessTokenLifetime,
                IdentityTokenLifetime = storefrontConfig.IdentityTokenLifetime
            };

            var oidcScopesWeb = new[] { "openid", "profile" };
            storefrontClient.AllowedScopes = (storefrontClient.AllowedScopes ?? new List<string>())
                .Concat(oidcScopesWeb)
                .Distinct()
                .ToList();

            if (storefrontConfig.AllowOfflineAccess)
            {
                storefrontClient.AllowedScopes ??= new List<string>();
                var allowedScopes = storefrontClient.AllowedScopes.ToList();
                if (!allowedScopes.Contains("offline_access"))
                {
                    allowedScopes.Add("offline_access");
                    storefrontClient.AllowedScopes = allowedScopes;
                }

                // Configure refresh token behavior for SPA using refresh tokens
                storefrontClient.RefreshTokenUsage = storefrontConfig.RefreshTokenUsage?.ToLower() switch
                {
                    "reuse" => TokenUsage.ReUse,
                    "onetimeonly" => TokenUsage.OneTimeOnly,
                    _ => TokenUsage.OneTimeOnly // Default to OneTimeOnly for security
                };
                storefrontClient.RefreshTokenExpiration = TokenExpiration.Sliding;
                if (storefrontConfig.AbsoluteRefreshTokenLifetime.HasValue && storefrontConfig.AbsoluteRefreshTokenLifetime.Value > 0)
                    storefrontClient.AbsoluteRefreshTokenLifetime = storefrontConfig.AbsoluteRefreshTokenLifetime.Value;
                if (storefrontConfig.SlidingRefreshTokenLifetime.HasValue && storefrontConfig.SlidingRefreshTokenLifetime.Value > 0)
                    storefrontClient.SlidingRefreshTokenLifetime = storefrontConfig.SlidingRefreshTokenLifetime.Value;
            }

            clients.Add(storefrontClient);
        }

        return clients;
    }
}
