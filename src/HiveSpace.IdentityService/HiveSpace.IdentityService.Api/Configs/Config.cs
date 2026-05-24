using Duende.IdentityServer.Models;

namespace HiveSpace.IdentityService.Api.Configs;

public static class Config
{
    public static IEnumerable<IdentityResource> IdentityResources =>
        [
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
        ];

    public static IEnumerable<ApiScope> ApiScopes =>
        [
            new ApiScope("identity.fullaccess", "Identity API Full Access") { UserClaims = { "sub", "name", "email", "role" } },
            new ApiScope("user.fullaccess", "User API Full Access") { UserClaims = { "sub", "name", "email", "role" } },
            new ApiScope("order.fullaccess", "Order API Full Access") { UserClaims = { "sub", "name", "email", "phone_number", "role" } },
            new ApiScope("basket.fullaccess", "Basket API Full Access") { UserClaims = { "sub", "name", "email", "role" } },
            new ApiScope("catalog.fullaccess", "Catalog API Full Access") { UserClaims = { "sub", "name", "email", "role" } },
            new ApiScope("media.fullaccess", "Media API Full Access") { UserClaims = { "sub", "name", "email", "role" } },
            new ApiScope("hivespace-backend.fullaccess", "Hivespace Backend API Full access") { UserClaims = { "sub", "name", "email", "role" } },
            new ApiScope("payment.fullaccess", "Payment API Full Access") { UserClaims = { "sub", "name", "email", "role" } },
            new ApiScope("notification.fullaccess", "Notification API Full Access") { UserClaims = { "sub", "name", "email", "role" } }
        ];

    public static IEnumerable<ApiResource> ApiResources =>
        [
            new ApiResource("identity", "Identity API")
            {
                Scopes = { "identity.fullaccess" },
                UserClaims = { "sub", "name", "email", "role" }
            },
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
            },
            new ApiResource("notification", "Notification API")
            {
                Scopes = { "notification.fullaccess" },
                UserClaims = { "sub", "name", "email", "role" }
            }
        ];

    public static IEnumerable<Client> GetClients(IConfiguration configuration)
    {
        var clients = new List<Client>();
        var clientsSection = configuration.GetSection("Clients");

        AddSpaClient(clients, clientsSection.GetSection("adminportal").Get<ClientConfig>());
        AddSpaClient(clients, clientsSection.GetSection("sellercenter").Get<ClientConfig>());
        AddSpaClient(clients, clientsSection.GetSection("storefront").Get<ClientConfig>());

        return clients;
    }

    private static void AddSpaClient(ICollection<Client> clients, ClientConfig? clientConfig)
    {
        if (clientConfig is null) return;

        var client = new Client
        {
            ClientId = clientConfig.ClientId,
            ClientName = clientConfig.ClientName,
            ClientUri = clientConfig.ClientUri,
            ClientSecrets = !string.IsNullOrEmpty(clientConfig.ClientSecret)
                ? [new Secret(clientConfig.ClientSecret.Sha256())]
                : [],
            RequireClientSecret = clientConfig.RequireClientSecret,
            AllowedGrantTypes = clientConfig.AllowedGrantTypes.Count > 0
                ? clientConfig.AllowedGrantTypes
                : ["authorization_code"],
            AllowAccessTokensViaBrowser = clientConfig.AllowAccessTokensViaBrowser,
            RequireConsent = clientConfig.RequireConsent,
            AllowOfflineAccess = clientConfig.AllowOfflineAccess,
            AlwaysIncludeUserClaimsInIdToken = clientConfig.AlwaysIncludeUserClaimsInIdToken,
            UpdateAccessTokenClaimsOnRefresh = true,
            RequirePkce = clientConfig.RequirePkce,
            RedirectUris = clientConfig.RedirectUris,
            PostLogoutRedirectUris = clientConfig.PostLogoutRedirectUris,
            AllowedCorsOrigins = clientConfig.AllowedCorsOrigins,
            AllowedScopes = clientConfig.AllowedScopes.Concat(["openid", "profile"]).Distinct().ToList(),
            AccessTokenLifetime = clientConfig.AccessTokenLifetime,
            IdentityTokenLifetime = clientConfig.IdentityTokenLifetime
        };

        if (clientConfig.AllowOfflineAccess)
        {
            client.AllowedScopes = client.AllowedScopes.Concat(["offline_access"]).Distinct().ToList();
            client.RefreshTokenUsage = clientConfig.RefreshTokenUsage?.ToLowerInvariant() switch
            {
                "reuse" => TokenUsage.ReUse,
                "onetimeonly" => TokenUsage.OneTimeOnly,
                _ => TokenUsage.OneTimeOnly
            };
            client.RefreshTokenExpiration = TokenExpiration.Sliding;

            if (clientConfig.AbsoluteRefreshTokenLifetime is > 0)
                client.AbsoluteRefreshTokenLifetime = clientConfig.AbsoluteRefreshTokenLifetime.Value;

            if (clientConfig.SlidingRefreshTokenLifetime is > 0)
                client.SlidingRefreshTokenLifetime = clientConfig.SlidingRefreshTokenLifetime.Value;
        }

        clients.Add(client);
    }
}
