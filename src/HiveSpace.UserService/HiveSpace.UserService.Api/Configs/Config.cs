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

        // WebApp Client (full config)
        var webappConfig = clientsSection.GetSection("webapp").Get<ClientConfig>();
        if (webappConfig != null)
        {
            var webappClient = new Client
            {
                ClientId = webappConfig.ClientId,
                ClientName = webappConfig.ClientName,
                ClientUri = webappConfig.ClientUri,
                ClientSecrets = !string.IsNullOrEmpty(webappConfig.ClientSecret)
                    ? new List<Secret> { new Secret(webappConfig.ClientSecret.Sha256()) }
                    : new List<Secret>(),
                RequireClientSecret = webappConfig.RequireClientSecret,
                AllowedGrantTypes = webappConfig.AllowedGrantTypes ?? ["authorization_code"],
                AllowAccessTokensViaBrowser = webappConfig.AllowAccessTokensViaBrowser,
                RequireConsent = webappConfig.RequireConsent,
                AllowOfflineAccess = webappConfig.AllowOfflineAccess,
                AlwaysIncludeUserClaimsInIdToken = webappConfig.AlwaysIncludeUserClaimsInIdToken,
                RequirePkce = webappConfig.RequirePkce,
                RedirectUris = webappConfig.RedirectUris ?? [],
                PostLogoutRedirectUris = webappConfig.PostLogoutRedirectUris ?? [],
                AllowedCorsOrigins = webappConfig.AllowedCorsOrigins ?? [],
                AllowedScopes = webappConfig.AllowedScopes ?? [],
                AccessTokenLifetime = webappConfig.AccessTokenLifetime,
                IdentityTokenLifetime = webappConfig.IdentityTokenLifetime
            };

            // Ensure offline_access scope is present when AllowOfflineAccess is true
            if (webappConfig.AllowOfflineAccess)
            {
                webappClient.AllowedScopes ??= new List<string>();
                var allowedScopes = webappClient.AllowedScopes.ToList();
                if (!allowedScopes.Contains("offline_access"))
                {
                    allowedScopes.Add("offline_access");
                    webappClient.AllowedScopes = allowedScopes;
                }
            }

            clients.Add(webappClient);
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
                AllowedGrantTypes = apiTestingConfig.AllowedGrantTypes ?? ["password"],
                AllowedScopes = apiTestingConfig.AllowedScopes ?? [],
                AlwaysIncludeUserClaimsInIdToken = apiTestingConfig.AlwaysIncludeUserClaimsInIdToken
            };
            clients.Add(apiTestingClient);
        }
        return clients;
    }
}
