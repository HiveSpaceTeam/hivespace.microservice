using System.Text.Json;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace HiveSpace.YarpApiGateway.Extensions;

internal static class ServiceCollectionExtensions
{
    private const int IdentityMetadataMaxAttempts = 10;
    private static readonly TimeSpan IdentityMetadataRetryDelay = TimeSpan.FromSeconds(2);

    public static void AddAppReverseProxy(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddReverseProxy()
            .LoadFromConfig(configuration.GetSection("ReverseProxy"));
    }

    public static void AddAppCors(this IServiceCollection services, IConfiguration configuration)
    {
        var allowedOrigins = configuration.GetSection("AllowedOrigins").Get<string[]>()
            ?? ["http://localhost:5174", "http://localhost:5173", "http://localhost:5175"];

        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.WithOrigins(allowedOrigins)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            });
        });
    }

    public static async Task AddAppGatewayTokenValidationAsync(this IServiceCollection services, IConfiguration configuration)
    {
        var validAudiences = configuration.GetSection("Authentication:ValidAudiences").Get<string[]>()
            ?? configuration.GetSection("BrowserSession:AccessTokenAudiences").Get<string[]>()
            ?? [];
        var authority = configuration["Authentication:Authority"];
        var requireHttpsMetadata = configuration.GetValue("Authentication:RequireHttpsMetadata", true);
        var metadataAddress = string.IsNullOrWhiteSpace(authority)
            ? null
            : $"{authority.TrimEnd('/')}/.well-known/openid-configuration";
        var (identityConfiguration, issuerSigningKeys) = metadataAddress is null
            ? (null, (ICollection<SecurityKey>)([]))
            : await LoadIdentityMetadataWithRetryAsync(metadataAddress, requireHttpsMetadata, CancellationToken.None);

        services.AddSingleton(new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = identityConfiguration?.Issuer ?? authority,
            ValidateAudience = validAudiences.Length > 0,
            ValidAudiences = validAudiences,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKeys = issuerSigningKeys,
            NameClaimType = "name",
            RoleClaimType = "role",
            ClockSkew = TimeSpan.FromMinutes(1)
        });
    }

    private static async Task<(OpenIdConnectConfiguration Configuration, ICollection<SecurityKey> SigningKeys)> LoadIdentityMetadataWithRetryAsync(
        string metadataAddress,
        bool requireHttpsMetadata,
        CancellationToken cancellationToken)
    {
        for (var attempt = 1; attempt <= IdentityMetadataMaxAttempts; attempt++)
        {
            try
            {
                var identityConfiguration = await OpenIdConnectConfigurationRetriever.GetAsync(
                    metadataAddress,
                    new HttpDocumentRetriever { RequireHttps = requireHttpsMetadata },
                    cancellationToken);
                ICollection<SecurityKey> issuerSigningKeys = identityConfiguration.SigningKeys.ToArray();

                if (issuerSigningKeys.Count == 0)
                {
                    using var httpClient = new HttpClient();
                    issuerSigningKeys = await LoadSigningKeysFromJwksAsync(httpClient, metadataAddress, cancellationToken);
                }

                if (identityConfiguration.SigningKeys.Count == 0)
                {
                    foreach (var signingKey in issuerSigningKeys)
                        identityConfiguration.SigningKeys.Add(signingKey);
                }

                return (identityConfiguration, issuerSigningKeys);
            }
            catch (Exception ex) when (attempt < IdentityMetadataMaxAttempts)
            {
                Console.WriteLine(
                    $"Identity metadata unavailable at {metadataAddress}. Retrying attempt {attempt + 1}/{IdentityMetadataMaxAttempts} in {IdentityMetadataRetryDelay.TotalSeconds}s. Error: {ex.Message}");
                await Task.Delay(IdentityMetadataRetryDelay, cancellationToken);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Unable to load Identity metadata from '{metadataAddress}' after {IdentityMetadataMaxAttempts} attempts.",
                    ex);
            }
        }

        throw new InvalidOperationException(
            $"Unable to load Identity metadata from '{metadataAddress}' after {IdentityMetadataMaxAttempts} attempts.");
    }

    private static async Task<ICollection<SecurityKey>> LoadSigningKeysFromJwksAsync(
        HttpClient httpClient,
        string metadataAddress,
        CancellationToken cancellationToken)
    {
        var metadataJson = await httpClient.GetStringAsync(metadataAddress, cancellationToken);
        using var metadataDocument = JsonDocument.Parse(metadataJson);
        var jwksUri = metadataDocument.RootElement.GetProperty("jwks_uri").GetString();
        if (string.IsNullOrWhiteSpace(jwksUri))
            throw new InvalidOperationException("Identity metadata does not declare jwks_uri.");

        var jwksJson = await httpClient.GetStringAsync(jwksUri, cancellationToken);
        return new JsonWebKeySet(jwksJson).Keys.Cast<SecurityKey>().ToArray();
    }

    public static void AddAppHealthChecks(this IServiceCollection services)
        => services.AddHealthChecks();
}
