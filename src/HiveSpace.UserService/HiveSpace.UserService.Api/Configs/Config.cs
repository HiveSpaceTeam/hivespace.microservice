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

        // dmin Portal Client (full config)
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
        var apiTestingConfig = clientsSection.GetSection("apitestingapp").Get<ClientConfig>();
        if (apiTestingConfig != null)
        {
            var apiTestingClient = new Client
            {
                ClientId = apiTestingConfig.ClientId,
                ClientName = apiTestingConfig.ClientName,
                ClientSecrets = !string.IsNullOrEmpty(apiTestingConfig.ClientSecret)
                    ? [new Secret(apiTestingConfig.ClientSecret.Sha256())]
                    : new List<Secret>(),
                RequireClientSecret = apiTestingConfig.RequireClientSecret,
                AllowedGrantTypes = apiTestingConfig.AllowedGrantTypes ?? ["client_credentials"],
                AllowedScopes = apiTestingConfig.AllowedScopes ?? [],
                AlwaysIncludeUserClaimsInIdToken = apiTestingConfig.AlwaysIncludeUserClaimsInIdToken
            };
            clients.Add(apiTestingClient);
        }
        return clients;
    }
}
